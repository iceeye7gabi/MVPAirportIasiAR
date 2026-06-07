#import <AVFoundation/AVFoundation.h>
#import <Speech/Speech.h>

static AVSpeechSynthesizer *synthesizer;
static SFSpeechRecognizer *recognizer;
static SFSpeechAudioBufferRecognitionRequest *recognitionRequest;
static SFSpeechRecognitionTask *recognitionTask;
static AVAudioEngine *audioEngine;
static NSString *callbackObjectName;
static NSString *callbackMethodName;
static NSString *bestPartialTranscript;
static NSTimer *listenTimeoutTimer;
static int audioLevelCounter = 0;

static void SendUnityMessage(NSString *payload) {
    if (callbackObjectName == nil || callbackMethodName == nil || payload == nil) {
        return;
    }

    dispatch_async(dispatch_get_main_queue(), ^{
        UnitySendMessage([callbackObjectName UTF8String], [callbackMethodName UTF8String], [payload UTF8String]);
    });
}

static float ComputeRms(AVAudioPCMBuffer *buffer) {
    if (buffer == nil || buffer.frameLength == 0 || buffer.floatChannelData == NULL) {
        return 0.f;
    }

    float *samples = buffer.floatChannelData[0];
    AVAudioFrameCount count = buffer.frameLength;
    float sum = 0.f;
    for (AVAudioFrameCount i = 0; i < count; i++) {
        sum += samples[i] * samples[i];
    }
    return sqrtf(sum / (float)count);
}

static void SendAudioLevel(float level) {
    SendUnityMessage([NSString stringWithFormat:@"LEVEL:%.3f", fminf(1.f, level)]);
}

static void ClearListenTimeout(void) {
    [listenTimeoutTimer invalidate];
    listenTimeoutTimer = nil;
}

static void StopListeningInternal(void) {
    ClearListenTimeout();

    if (recognitionTask != nil) {
        [recognitionTask cancel];
        recognitionTask = nil;
    }

    if (audioEngine != nil && audioEngine.isRunning) {
        [audioEngine stop];
        [audioEngine.inputNode removeTapOnBus:0];
    }

    if (recognitionRequest != nil) {
        [recognitionRequest endAudio];
        recognitionRequest = nil;
    }

    bestPartialTranscript = nil;
}

static void FinishListeningWithMessage(NSString *message) {
    StopListeningInternal();
    if (message != nil && message.length > 0) {
        SendUnityMessage(message);
    }
}

static AVSpeechSynthesisVoice *PickVoice(NSString *lang) {
    AVSpeechSynthesisVoice *voice = [AVSpeechSynthesisVoice voiceWithLanguage:lang];
    if (voice != nil) {
        return voice;
    }

    voice = [AVSpeechSynthesisVoice voiceWithLanguage:@"ro-RO"];
    if (voice != nil) {
        return voice;
    }

    voice = [AVSpeechSynthesisVoice voiceWithLanguage:@"ro"];
    if (voice != nil) {
        return voice;
    }

    for (AVSpeechSynthesisVoice *candidate in [AVSpeechSynthesisVoice speechVoices]) {
        if ([candidate.language hasPrefix:@"ro"]) {
            return candidate;
        }
    }

    return [AVSpeechSynthesisVoice voiceWithLanguage:@"en-US"];
}

static void ConfigurePlaybackSession(void) {
    AVAudioSession *session = [AVAudioSession sharedInstance];
    NSError *error = nil;

    // PlayAndRecord + speaker works better when Unity WebCamTexture holds the session.
    [session setCategory:AVAudioSessionCategoryPlayAndRecord
                    mode:AVAudioSessionModeDefault
                 options:AVAudioSessionCategoryOptionDefaultToSpeaker |
                         AVAudioSessionCategoryOptionMixWithOthers |
                         AVAudioSessionCategoryOptionAllowBluetooth
                   error:&error];
    if (error != nil) {
        NSLog(@"[IosSpeechBridge] setCategory error: %@", error);
        error = nil;
    }

    [session setActive:YES withOptions:AVAudioSessionSetActiveOptionNotifyOthersOnDeactivation error:&error];
    if (error != nil) {
        NSLog(@"[IosSpeechBridge] setActive error: %@", error);
        error = nil;
    }

    [session overrideOutputAudioPort:AVAudioSessionPortOverrideSpeaker error:&error];
    if (error != nil) {
        NSLog(@"[IosSpeechBridge] overrideOutputAudioPort error: %@", error);
    }
}

extern "C" {

void _SpeechPreparePlayback(void) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (synthesizer == nil) {
            synthesizer = [[AVSpeechSynthesizer alloc] init];
        }
        ConfigurePlaybackSession();
    });
}

void _SpeechSpeak(const char* text, const char* language) {
    if (text == NULL) return;

    dispatch_async(dispatch_get_main_queue(), ^{
        if (synthesizer == nil) {
            synthesizer = [[AVSpeechSynthesizer alloc] init];
        }

        [synthesizer stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];
        ConfigurePlaybackSession();

        NSString *message = [NSString stringWithUTF8String:text];
        if (message.length == 0) {
            return;
        }

        AVSpeechUtterance *utterance = [AVSpeechUtterance speechUtteranceWithString:message];
        NSString *lang = language != NULL ? [NSString stringWithUTF8String:language] : @"ro-RO";
        AVSpeechSynthesisVoice *voice = PickVoice(lang);
        if (voice != nil) {
            utterance.voice = voice;
        }

        utterance.rate = AVSpeechUtteranceDefaultSpeechRate * 0.9f;
        utterance.pitchMultiplier = 1.f;
        utterance.volume = 1.f;
        utterance.preUtteranceDelay = 0.05f;

        NSLog(@"[IosSpeechBridge] Speaking (%lu chars), voice=%@", (unsigned long)message.length, voice.language);
        [synthesizer speakUtterance:utterance];
    });
}

void _SpeechStop() {
    dispatch_async(dispatch_get_main_queue(), ^{
        [synthesizer stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];
    });
}

void _SpeechStopListening() {
    StopListeningInternal();
}

void _SpeechStartListening(const char* gameObjectName, const char* methodName, const char* language) {
    if (gameObjectName == NULL || methodName == NULL) return;

    callbackObjectName = [NSString stringWithUTF8String:gameObjectName];
    callbackMethodName = [NSString stringWithUTF8String:methodName];
    NSString *lang = language != NULL ? [NSString stringWithUTF8String:language] : @"ro-RO";
    bestPartialTranscript = nil;

    [SFSpeechRecognizer requestAuthorization:^(SFSpeechRecognizerAuthorizationStatus authStatus) {
        if (authStatus != SFSpeechRecognizerAuthorizationStatusAuthorized) {
            SendUnityMessage(@"ERROR:Permisiune recunoaștere vocală refuzată.");
            return;
        }

        [[AVAudioSession sharedInstance] requestRecordPermission:^(BOOL granted) {
            if (!granted) {
                SendUnityMessage(@"ERROR:Permisiune microfon refuzată.");
                return;
            }

            dispatch_async(dispatch_get_main_queue(), ^{
                StopListeningInternal();

                NSLocale *locale = [NSLocale localeWithLocaleIdentifier:lang];
                recognizer = [[SFSpeechRecognizer alloc] initWithLocale:locale];
                if (recognizer == nil || !recognizer.isAvailable) {
                    recognizer = [[SFSpeechRecognizer alloc] initWithLocale:[NSLocale localeWithLocaleIdentifier:@"en-US"]];
                }

                if (recognizer == nil || !recognizer.isAvailable) {
                    SendUnityMessage(@"ERROR:Recunoașterea vocală nu este disponibilă pe acest dispozitiv.");
                    return;
                }

                AVAudioSession *session = [AVAudioSession sharedInstance];
                NSError *sessionError = nil;
                [session setActive:NO withOptions:AVAudioSessionSetActiveOptionNotifyOthersOnDeactivation error:nil];
                [session setCategory:AVAudioSessionCategoryPlayAndRecord
                                mode:AVAudioSessionModeDefault
                             options:AVAudioSessionCategoryOptionDefaultToSpeaker |
                                     AVAudioSessionCategoryOptionAllowBluetooth |
                                     AVAudioSessionCategoryOptionMixWithOthers
                               error:&sessionError];
                [session setActive:YES withOptions:0 error:&sessionError];
                if (sessionError != nil) {
                    SendUnityMessage(@"ERROR:Nu am putut activa sesiunea audio.");
                    return;
                }

                recognitionRequest = [[SFSpeechAudioBufferRecognitionRequest alloc] init];
                recognitionRequest.shouldReportPartialResults = YES;
                if (@available(iOS 13.0, *)) {
                    recognitionRequest.requiresOnDeviceRecognition = NO;
                }

                audioEngine = [[AVAudioEngine alloc] init];
                AVAudioInputNode *inputNode = audioEngine.inputNode;
                AVAudioFormat *format = [inputNode outputFormatForBus:0];
                if (format == nil || format.sampleRate <= 0) {
                    format = [[AVAudioFormat alloc] initWithCommonFormat:AVAudioPCMFormatFloat32
                                                              sampleRate:44100
                                                                channels:1
                                                             interleaved:NO];
                }

                [inputNode removeTapOnBus:0];
                audioLevelCounter = 0;
                [inputNode installTapOnBus:0 bufferSize:4096 format:format block:^(AVAudioPCMBuffer *buffer, AVAudioTime *when) {
                    if (recognitionRequest != nil) {
                        [recognitionRequest appendAudioPCMBuffer:buffer];
                    }

                    audioLevelCounter++;
                    if (audioLevelCounter % 3 == 0) {
                        float rms = ComputeRms(buffer);
                        SendAudioLevel(rms * 14.f);
                    }
                }];

                [audioEngine prepare];
                NSError *startError = nil;
                [audioEngine startAndReturnError:&startError];
                if (startError != nil) {
                    SendUnityMessage(@"ERROR:Nu am putut porni microfonul.");
                    return;
                }

                listenTimeoutTimer = [NSTimer scheduledTimerWithTimeInterval:10.0
                                                                     repeats:NO
                                                                       block:^(__unused NSTimer *timer) {
                    if (bestPartialTranscript != nil && bestPartialTranscript.length > 0) {
                        FinishListeningWithMessage(bestPartialTranscript);
                    } else {
                        FinishListeningWithMessage(@"ERROR:Nu am detectat voce. Vorbește mai aproape de microfon, apoi încearcă din nou.");
                    }
                }];

                recognitionTask = [recognizer recognitionTaskWithRequest:recognitionRequest
                                                           resultHandler:^(SFSpeechRecognitionResult *result, NSError *error) {
                    if (result != nil) {
                        NSString *text = result.bestTranscription.formattedString;
                        if (text.length > 0) {
                            bestPartialTranscript = text;
                        }

                        if (result.isFinal && text.length > 0) {
                            FinishListeningWithMessage(text);
                        }
                        return;
                    }

                    if (error == nil) {
                        return;
                    }

                    if (error.code == 216 || error.code == 301) {
                        return;
                    }

                    if (bestPartialTranscript != nil && bestPartialTranscript.length > 0) {
                        FinishListeningWithMessage(bestPartialTranscript);
                        return;
                    }

                    if (error.code == 1110) {
                        FinishListeningWithMessage(@"ERROR:Nu am detectat voce. Spune clar, de exemplu: check-in, stânga sau dreapta.");
                    } else {
                        FinishListeningWithMessage([NSString stringWithFormat:@"ERROR:%@", error.localizedDescription]);
                    }
                }];
            });
        }];
    }];
}

}

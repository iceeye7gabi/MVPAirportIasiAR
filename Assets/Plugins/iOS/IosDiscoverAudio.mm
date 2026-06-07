#import <AVFoundation/AVFoundation.h>

static AVAudioPlayer *discoverPlayer;

static void ConfigureDiscoverPlaybackSession(void) {
    AVAudioSession *session = [AVAudioSession sharedInstance];
    NSError *error = nil;

    [session setCategory:AVAudioSessionCategoryPlayAndRecord
                    mode:AVAudioSessionModeDefault
                 options:AVAudioSessionCategoryOptionDefaultToSpeaker |
                         AVAudioSessionCategoryOptionMixWithOthers
                   error:&error];
    if (error != nil) {
        NSLog(@"[IosDiscoverAudio] setCategory error: %@", error);
        error = nil;
    }

    [session setActive:YES error:&error];
    if (error != nil) {
        NSLog(@"[IosDiscoverAudio] setActive error: %@", error);
        error = nil;
    }

    [session overrideOutputAudioPort:AVAudioSessionPortOverrideSpeaker error:&error];
    if (error != nil) {
        NSLog(@"[IosDiscoverAudio] overrideOutputAudioPort error: %@", error);
    }
}

extern "C" {

void _DiscoverPlayAudioFile(const char *path) {
    if (path == NULL) {
        NSLog(@"[IosDiscoverAudio] path is NULL");
        return;
    }

    NSLog(@"[IosDiscoverAudio] request path: %s", path);

    dispatch_async(dispatch_get_main_queue(), ^{
        if (discoverPlayer != nil) {
            [discoverPlayer stop];
            discoverPlayer = nil;
        }

        NSString *filePath = [NSString stringWithUTF8String:path];
        if (filePath == nil || filePath.length == 0) {
            NSLog(@"[IosDiscoverAudio] invalid UTF8 path");
            return;
        }

        if (![[NSFileManager defaultManager] fileExistsAtPath:filePath]) {
            NSLog(@"[IosDiscoverAudio] missing file: %@", filePath);
            return;
        }

        ConfigureDiscoverPlaybackSession();

        NSURL *url = [NSURL fileURLWithPath:filePath];
        NSError *error = nil;
        discoverPlayer = [[AVAudioPlayer alloc] initWithContentsOfURL:url error:&error];
        if (error != nil || discoverPlayer == nil) {
            NSLog(@"[IosDiscoverAudio] init error: %@", error);
            return;
        }

        discoverPlayer.volume = 1.f;
        [discoverPlayer prepareToPlay];
        BOOL started = [discoverPlayer play];
        NSLog(@"[IosDiscoverAudio] play %@ started=%d duration=%.2fs",
              url.lastPathComponent, started, discoverPlayer.duration);
    });
}

void _DiscoverStopAudio(void) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (discoverPlayer != nil) {
            [discoverPlayer stop];
            discoverPlayer = nil;
        }
    });
}

}

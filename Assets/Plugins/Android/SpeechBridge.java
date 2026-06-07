package com.airportar.speech;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.speech.RecognitionListener;
import android.speech.RecognizerIntent;
import android.speech.SpeechRecognizer;
import android.speech.tts.TextToSpeech;

import com.unity3d.player.UnityPlayer;

import java.util.ArrayList;
import java.util.Locale;

public class SpeechBridge {
    private static TextToSpeech tts;
    private static SpeechRecognizer recognizer;
    private static String callbackObject;
    private static String callbackMethod;

    public static void init(final Activity activity) {
        if (tts != null) return;
        tts = new TextToSpeech(activity, status -> {
            if (status == TextToSpeech.SUCCESS) {
                tts.setLanguage(new Locale("ro", "RO"));
            }
        });
    }

    public static void speak(String text, String languageTag) {
        if (tts == null) return;
        Locale locale = Locale.forLanguageTag(languageTag != null ? languageTag : "ro-RO");
        tts.setLanguage(locale);
        tts.speak(text, TextToSpeech.QUEUE_FLUSH, null, "airport_ar_guide");
    }

    public static void stopSpeak() {
        if (tts != null) {
            tts.stop();
        }
    }

    public static void startListening(Activity activity, String unityObject, String unityMethod, String languageTag) {
        callbackObject = unityObject;
        callbackMethod = unityMethod;

        if (recognizer != null) {
            recognizer.destroy();
        }

        recognizer = SpeechRecognizer.createSpeechRecognizer(activity);
        recognizer.setRecognitionListener(new RecognitionListener() {
            @Override public void onReadyForSpeech(Bundle params) {}
            @Override public void onBeginningOfSpeech() {}
            @Override
            public void onRmsChanged(float rmsdB) {
                float normalized = Math.min(1f, Math.max(0f, (rmsdB + 2f) / 12f));
                sendUnity("LEVEL:" + String.format(java.util.Locale.US, "%.3f", normalized));
            }
            @Override public void onBufferReceived(byte[] buffer) {}
            @Override public void onEndOfSpeech() {}
            @Override public void onEvent(int eventType, Bundle params) {}

            @Override
            public void onError(int error) {
                sendUnity("ERROR:Recunoașere vocală eșuată.");
            }

            @Override
            public void onResults(Bundle results) {
                ArrayList<String> matches = results.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION);
                if (matches != null && !matches.isEmpty()) {
                    sendUnity(matches.get(0));
                } else {
                    sendUnity("ERROR:Nu am auzit nimic.");
                }
            }

            @Override
            public void onPartialResults(Bundle partialResults) {}
        });

        Intent intent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE, languageTag != null ? languageTag : "ro-RO");
        intent.putExtra(RecognizerIntent.EXTRA_MAX_RESULTS, 1);
        recognizer.startListening(intent);
    }

    public static void stopListening() {
        if (recognizer != null) {
            recognizer.stopListening();
        }
    }

    private static void sendUnity(String payload) {
        if (callbackObject != null && callbackMethod != null) {
            UnityPlayer.UnitySendMessage(callbackObject, callbackMethod, payload);
        }
    }
}

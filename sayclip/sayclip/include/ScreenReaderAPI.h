// Copyright © 2011, QuentinC http://quentinc.net/
// Last change : 17.09.2011

#ifndef _____SCREEN_READERS_API_H
#define _____SCREEN_READERS_API_H
#include<windows.h>

#define export __stdcall

/* Auto ANSI/Unicode naming, coherent with general windows API functions */
#ifdef UNICODE
#define sayString sayStringW
#define jfwSayString jfwSayStringW
#define jfwRunScript jfwRunScriptW
#define jfwRunFunction jfwRunFunctionW
#define weSayString weSayStringW
#define saSayString saSayStringW
#define sapiSayString sapiSayStringW
#define sapiSaySSML sapiSaySSMLW
#define sapiGetVoiceName sapiGetVoiceNameW
#else
#define sayString sayStringA
#define jfwSayString jfwSayStringA
#define jfwRunScript jfwRunScriptA
#define jfwRunFunction jfwRunFunctionA
#define weSayString weSayStringA
#define saSayString saSayStringA
#define sapiSayString sapiSayStringA
#define sapiSaySSML sapiSaySSMLA
#define sapiGetVoiceName sapiGetVoiceNameA
#endif

/* Screen reader types constants to use with get/setCurrentScreenReader */
// No screen reader or none loaded at the moment
#define SR_NONE 0
// Jaws for windows
#define SR_JFW 1
// Non visual desktop access
#define SR_NVDA 2
// Windows eye
#define SR_WE 3
// System access
#define SR_SA 4
// Windows Speech API (SAPI5)
#define SR_SAPI 15

#ifdef __cplusplus
extern "C" {
#endif

/* Global functions. These should be used preferably to the specific ones so that you are independent of the screen reader used by the end user. */

/* Speak a string of text and tell if currently speaking text should be interrupted or not. 
* With true as second argument, the given string of text is immediately speaking, interrupting what was speaking before
* With false, the given string is appended to the queue of messages to speak, the current message being spoken is not interrupted
*/
BOOL export sayStringA (const char* string, BOOL interrupt) ;
 
/* unicode version of the function above */
BOOL export sayStringW (const wchar_t* unicodeString, BOOL interrupt) ; 

/* Immediately stops any spoken text */
BOOL export stopSpeech (void) ;

/* Tell or define which scrren reader is currently used by the global functions above. See SR_* constants for values meaning */
 int export getCurrentScreenReader (void) ;
int export setCurrentScreenReader (int screenReaderID) ;

/* Tell or define if SAPI5 must be used as fallback if no any other screen reader is found. Default is FALSE.  */
BOOL export sapiIsEnabled (void) ;
BOOL export sapiEnable (BOOL enable) ;

// Jaws specific functions
int export jfwLoad (void) ;
int export jfwIsActive (void) ;
int export jfwSayStringA (const char* string, int interrupt) ;
int export jfwSayStringW (const wchar_t* str, int interrupt) ;
int export jfwStopSpeech (void) ;
int export jfwRunScriptA (const char* scriptName) ;
int export jfwRunScriptW (const wchar_t* scriptName) ;
int export jfwRunFunctionA (const char* funcName) ;
int export jfwRunFunctionW (const wchar_t* funcName) ;
void export jfwUnload (void) ;

// Windows eye specific functions
int export weLoad (void) ;
int export weIsActive (void) ;
int export weSayStringA (const char* string) ;
int export weSayStringW (const wchar_t* string) ;
int export weStopSpeech (void) ;
void export weUnload (void) ;

// NVDA specific functions
int export nvdaLoad (void) ;
int export nvdaIsActive (void) ;
int export nvdaSayString (const wchar_t* unicodeString) ;
int export nvdaStopSpeech (void) ;
int export nvdaBrailleMessage (const wchar_t* unicodeString) ;
void export nvdaUnload (void) ;

// System access specific functions
int export saLoad (void) ;
int export saIsActive (void) ;
int export saSayStringA (const char* string) ;
int export saSayStringW (const wchar_t* unicodeString) ;
int export saStopSpeech (void) ;
void export saUnload (void) ;

// Types to use with SAPI advanced functions
/* Output callback to be used with sapiSetOutputCallback (see below). First parameter=user data, second parameter=data buffer, third parameter=buffer length in bytes, return value=number of bytes handled, should normally be equal to length given */
typedef int(*SapiWaveOutputCallback)(void*, void*, int) ;

// SAPI5 specific functions
BOOL export sapiLoad () ;
void export sapiUnload () ;
/* SAPI say functions */
BOOL export sapiSayStringW (const WCHAR* str, BOOL interrupt) ;
BOOL export sapiSaySSMLW (const WCHAR* str, BOOL interrupt) ; // See MSDN if you want to work with SSML speech synthesis markup language
BOOL export sapiSayStringA (const char* str, BOOL interrupt) ;
BOOL export sapiSaySSMLA (const char* str, BOOL interrupt) ;
BOOL sapiStopSpeech (void) ;
/* Set speech rate between 0 (slowest) to 100 (fastest). 50=middle. */
BOOL export sapiSetRate (int rate) ; 
/* Return current speech rate between 0 and 100, negative on error or if that information is unavailable */
int export sapiGetRate () ;
/* Set the speech volume between 0 (quietest) to 100 (loudest) */
BOOL export sapiSetVolume (int volume) ; 
/* Return the current speech volume between 0 and 100, negative on error or if that information is unavailable  */
int export sapiGetVolume () ; 
/* Tell or set the pause status of the speech */
BOOL export sapiSetPaused (BOOL paused) ; 
BOOL export sapiIsPaused () ;
/* Wait for SAPI to complete its speech up to the specified number of milliseconds.  */
BOOL export sapiWait (int timeout) ; 
/* Tell if SAPI is currently speaking */
BOOL export sapiIsSpeaking () ; 
/* Return the number of SAPI voices availables in the system */
int export sapiGetNumVoices () ; 
/* SEt the speaking voice, use 0-based index */
BOOL export sapiSetVoice (int n) ; 
/* Return the 0-based index of the current SAPI voice used, -1 for default, <=-2 on error or if that information is unavailable. */
int export sapiGetVoice () ; 
/* Return the name of the nth voice. Don't forget to free the returned string after use */
const wchar_t* export sapiGetVoiceNameW (int n) ;
/* ANSI version of the function above */
const char* export sapiGetVoiceNameA (int n) ;
/* Set an output callback function that will receive audio data produced by the SAPI engine instead of the default sound card, at the sample rate specified, mono 16 bit signed. Set NULL as callback function to disable redirection and speak again on the default sound card */
BOOL export sapiSetOutputCallback (int sampleRate, SapiWaveOutputCallback callback, void* udata) ;

#ifdef __cplusplus
} // extern "C"
#endif

#endif


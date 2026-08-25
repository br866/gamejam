/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID PLAY_CHECKPOINTSFX = 1671843005U;
        static const AkUniqueID PLAY_CRATE_PUSH = 470824146U;
        static const AkUniqueID PLAY_DEATHCAUSE_MUSIC = 2229278265U;
        static const AkUniqueID PLAY_DOOR_LOCKED = 3779672947U;
        static const AkUniqueID PLAY_DOOR_UNLOCKING = 4191924555U;
        static const AkUniqueID PLAY_FLUORESCENT_LIGHT = 3799125421U;
        static const AkUniqueID PLAY_FOOTSTEP_BRUTEDOC = 3546231729U;
        static const AkUniqueID PLAY_FOOTSTEP_DOG = 1441942263U;
        static const AkUniqueID PLAY_FOOTSTEP_HUMAN = 1846914642U;
        static const AkUniqueID PLAY_GAMEPLAY_MUSIC = 1231987938U;
        static const AkUniqueID PLAY_KEY_PICKUP = 2881789206U;
        static const AkUniqueID PLAY_LEVEL5_MUSIC = 3337214613U;
        static const AkUniqueID PLAY_PLAYERDEATH_STINGER = 2509310786U;
        static const AkUniqueID PLAY_PRESSUREPLATE = 2541206429U;
        static const AkUniqueID PLAY_TITLE_MUSIC = 2604896900U;
        static const AkUniqueID PLAY_UI_CLICK = 1749424733U;
        static const AkUniqueID PLAY_UI_HOVER = 1339559671U;
        static const AkUniqueID PLAY_UI_PARCHMENT_CLOSE = 3581212516U;
        static const AkUniqueID PLAY_UI_PARCHMENT_OPEN = 1586167264U;
        static const AkUniqueID STOP_CRATE_PUSH = 708504560U;
        static const AkUniqueID STOP_DEATHCAUSE_MUSIC = 1522432571U;
        static const AkUniqueID STOP_GAMEPLAY_MUSIC = 3536547992U;
        static const AkUniqueID STOP_LEVEL5_MUSIC = 599953395U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace ANXIETYLEVEL
        {
            static const AkUniqueID GROUP = 1446207487U;

            namespace STATE
            {
                static const AkUniqueID HIGH = 3550808449U;
                static const AkUniqueID LOW = 545371365U;
                static const AkUniqueID MID = 1182670505U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace ANXIETYLEVEL

        namespace COD
        {
            static const AkUniqueID GROUP = 1083682085U;

            namespace STATE
            {
                static const AkUniqueID ANXIETY = 4143496951U;
                static const AkUniqueID ELIMINATED = 1342227465U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace COD

        namespace MUSICMODE
        {
            static const AkUniqueID GROUP = 222311475U;

            namespace STATE
            {
                static const AkUniqueID COMBAT = 2764240573U;
                static const AkUniqueID EXPLORE = 579523862U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace MUSICMODE

    } // namespace STATES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID MUSICVOLUME = 2346531308U;
        static const AkUniqueID SFXVOLUME = 988953028U;
    } // namespace GAME_PARAMETERS

    namespace BUSSES
    {
        static const AkUniqueID AMBIENCE = 85412153U;
        static const AkUniqueID MAIN_AUDIO_BUS = 2246998526U;
        static const AkUniqueID MUSIC = 3991942870U;
        static const AkUniqueID SFX = 393239870U;
        static const AkUniqueID UI = 1551306167U;
    } // namespace BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__

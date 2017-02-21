/* ========================================================================================== */
/* FMOD System - C# Wrapper . Copyright (c), Firelight Technologies Pty, Ltd. 2004-2016.      */
/*                                                                                            */
/*                                                                                            */
/* ========================================================================================== */

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace FMOD.Studio
{
    public class STUDIO_VERSION
    {
#if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        public const string dll    = "__Internal";
#elif (UNITY_PS4 || UNITY_WIIU || UNITY_PSP2) && !UNITY_EDITOR
		public const string dll    = "libfmodstudio";
#elif UNITY_EDITOR || ((UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX) && DEVELOPMENT_BUILD)
        public const string dll    = "fmodstudiol";
#else
		public const string dll    = "fmodstudio";
#endif
    }

    public enum STOP_MODE
    {
        ALLOWFADEOUT,              /* Allows AHDSR modulators to complete their release, and DSP effect tails to play out. */
        IMMEDIATE,                 /* Stops the event instance immediately. */
    }

    public enum LOADING_STATE
    {
        UNLOADING,        /* Currently unloading. */
        UNLOADED,         /* Not loaded. */
        LOADING,          /* Loading in progress. */
        LOADED,           /* Loaded and ready to play. */
        ERROR,            /* Failed to load and is now in error state. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROGRAMMER_SOUND_PROPERTIES
    {
        public StringWrapper name;
        public IntPtr sound;
        public int subsoundIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TIMELINE_MARKER_PROPERTIES
    {
        public StringWrapper name;
        public int position;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TIMELINE_BEAT_PROPERTIES
    {
        public int bar;
        public int beat;
        public int position;
        public float tempo;
        public int timeSignatureUpper;
        public int timeSignatureLower;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ADVANCEDSETTINGS
    {
        public int cbSize;               /* [w]   Size of this structure.  NOTE: For C# wrapper, users can leave this at 0. ! */
        public int commandQueueSize;     /* [r/w] Optional. Specify 0 to ignore. Specify the command queue size for studio async processing.  Default 4096 (4kb) */
        public int handleInitialSize;    /* [r/w] Optional. Specify 0 to ignore. Specify the initial size to allocate for handles.  Memory for handles will grow as needed in pages. */
        public int studioUpdatePeriod;   /* [r/w] Optional. Specify 0 to ignore. Specify the update period of Studio when in async mode, in milliseconds.  Will be quantised to the nearest multiple of mixer duration.  Default is 20ms. */
        public int idleSampleDataPoolSize; /* [r/w] Optional. Specify 0 to ignore. Specify the amount of sample data to keep in memory when no longer used, to avoid repeated disk IO.  Use -1 to disable.  Default is 256kB. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CPU_USAGE
    {
        public float dspUsage;            /* Returns the % CPU time taken by DSP processing on the low level mixer thread. */
        public float streamUsage;         /* Returns the % CPU time taken by stream processing on the low level stream thread. */
        public float geometryUsage;       /* Returns the % CPU time taken by geometry processing on the low level geometry thread. */
        public float updateUsage;         /* Returns the % CPU time taken by low level update, called as part of the studio update. */
        public float studioUsage;         /* Returns the % CPU time taken by studio update, called from the studio thread. Does not include low level update time. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BUFFER_INFO
    {
        public int currentUsage;                    /* Current buffer usage in bytes. */
        public int peakUsage;                       /* Peak buffer usage in bytes. */
        public int capacity;                        /* Buffer capacity in bytes. */
        public int stallCount;                      /* Number of stalls due to buffer overflow. */
        public float stallTime;                     /* Amount of time stalled due to buffer overflow, in seconds. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BUFFER_USAGE
    {
        public BUFFER_INFO studioCommandQueue;      /* Information for the Studio Async Command buffer, controlled by FMOD_STUDIO_ADVANCEDSETTINGS commandQueueSize. */
        public BUFFER_INFO studioHandle;            /* Information for the Studio handle table, controlled by FMOD_STUDIO_ADVANCEDSETTINGS handleInitialSize. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BANK_INFO
    {
        public int size;                            /* The size of this struct (for binary compatibility) */
        public IntPtr userData;                     /* User data to be passed to the file callbacks */
        public int userDataLength;                  /* If this is non-zero, userData will be copied internally */
        public FILE_OPENCALLBACK openCallback;      /* Callback for opening this file. */
        public FILE_CLOSECALLBACK closeCallback;    /* Callback for closing this file. */
        public FILE_READCALLBACK readCallback;      /* Callback for reading from this file. */
        public FILE_SEEKCALLBACK seekCallback;      /* Callback for seeking within this file. */
    }

    [Flags]
    public enum SYSTEM_CALLBACK_TYPE : uint
    {
        PREUPDATE       = 0x00000001,  /* Called at the start of the main Studio update.  For async mode this will be on its own thread. */
        POSTUPDATE      = 0x00000002,  /* Called at the end of the main Studio update.  For async mode this will be on its own thread. */
        BANK_UNLOAD     = 0x00000004,  /* Called when bank has just been unloaded, after all resources are freed. CommandData will be the bank handle.*/
        ALL             = 0xFFFFFFFF,  /* Pass this mask to Studio::System::setCallback to receive all callback types. */
    }

    public delegate RESULT SYSTEM_CALLBACK(IntPtr systemraw, SYSTEM_CALLBACK_TYPE type, IntPtr parameters, IntPtr userdata);

    public enum PARAMETER_TYPE
    {
        GAME_CONTROLLED,                  /* Controlled via the API using Studio::ParameterInstance::setValue. */
        AUTOMATIC_DISTANCE,               /* Distance between the event and the listener. */
        AUTOMATIC_EVENT_CONE_ANGLE,       /* Angle between the event's forward vector and the vector pointing from the event to the listener (0 to 180 degrees). */
        AUTOMATIC_EVENT_ORIENTATION,      /* Horizontal angle between the event's forward vector and listener's forward vector (-180 to 180 degrees). */
        AUTOMATIC_DIRECTION,              /* Horizontal angle between the listener's forward vector and the vector pointing from the listener to the event (-180 to 180 degrees). */
        AUTOMATIC_ELEVATION,              /* Angle between the listener's XZ plane and the vector pointing from the listener to the event (-90 to 90 degrees). */
        AUTOMATIC_LISTENER_ORIENTATION,   /* Horizontal angle between the listener's forward vector and the global positive Z axis (-180 to 180 degrees). */
    }

    public struct PARAMETER_DESCRIPTION
    {
        public string name;                                /* Name of the parameter. */
        public int index;                                  /* Index of the parameter */
        public float minimum;                              /* Minimum parameter value. */
        public float maximum;                              /* Maximum parameter value. */
        public float defaultValue;                         /* Default parameter value. */
        public PARAMETER_TYPE type;                        /* Type of the parameter */
    }

    #region wrapperinternal

    // The above structure has an issue with getting a const char* back from game code so we use this special marshalling struct instead
    [StructLayout(LayoutKind.Sequential)]
    struct PARAMETER_DESCRIPTION_INTERNAL
    {
        public IntPtr name;                                /* Name of the parameter. */
        public int index;                                  /* Index of the parameter */
        public float minimum;                              /* Minimum parameter value. */
        public float maximum;                              /* Maximum parameter value. */
        public float defaultValue;                         /* Default parameter value. */
        public PARAMETER_TYPE type;                        /* Type of the parameter */

        // Helper functions
        public void assign(out PARAMETER_DESCRIPTION publicDesc)
        {
            publicDesc.name = MarshallingHelper.stringFromNativeUtf8(name);
            publicDesc.index = index;
            publicDesc.minimum = minimum;
            publicDesc.maximum = maximum;
            publicDesc.defaultValue = defaultValue;
            publicDesc.type = type;
        }
    }
    // This is only need for loading memory and given our C# wrapper LOAD_MEMORY_POINT isn't feasible anyway
    enum LOAD_MEMORY_MODE
    {
        LOAD_MEMORY,
        LOAD_MEMORY_POINT,
    }

    #endregion

    public class SOUND_INFO
    {
        public byte[] name_or_data;         /* The filename or memory buffer that contains the sound. */
        public MODE mode;                   /* Mode flags required for loading the sound. */
        public CREATESOUNDEXINFO exinfo;    /* Extra information required for loading the sound. */
        public int subsoundIndex;           /* Subsound index for loading the sound. */

        // For informational purposes - returns null if the sound will be loaded from memory
        public string name
        {
            get
            {
                if (((mode & (MODE.OPENMEMORY | MODE.OPENMEMORY_POINT)) == 0) && (name_or_data != null))
                {
                    int strlen = Array.IndexOf(name_or_data, (byte)0);
                    if (strlen > 0)
                    {
                        return Encoding.UTF8.GetString(name_or_data, 0, strlen);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        #region wrapperinternal

        ~SOUND_INFO()
        {
            if (exinfo.inclusionlist != IntPtr.Zero)
            {
                // Allocated in SOUND_INFO_INTERNAL::assign()
                Marshal.FreeHGlobal(exinfo.inclusionlist);
            }
        }

        #endregion
    }

    #region wrapperinternal

    // The SOUND_INFO class has issues with getting pointers back from game code so we use this special marshalling struct instead
    [StructLayout(LayoutKind.Sequential)]
    public struct SOUND_INFO_INTERNAL
    {
        IntPtr name_or_data;
        MODE mode;
        CREATESOUNDEXINFO_INTERNAL exinfo;
        int subsoundIndex;

        // Helper functions
        public void assign(out SOUND_INFO publicInfo)
        {
            publicInfo = new SOUND_INFO();

            publicInfo.mode = mode;
            publicInfo.exinfo = CREATESOUNDEXINFO_INTERNAL.CreateFromInternal(ref exinfo);

            // Somewhat hacky: we know the inclusion list always points to subsoundIndex, so recreate it here
            #if NETFX_CORE
            publicInfo.exinfo.inclusionlist = Marshal.AllocHGlobal(Marshal.SizeOf<Int32>());
            #else
            publicInfo.exinfo.inclusionlist = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int32)));
            #endif
            Marshal.WriteInt32(publicInfo.exinfo.inclusionlist, subsoundIndex);
            publicInfo.exinfo.inclusionlistnum = 1;

            publicInfo.subsoundIndex = subsoundIndex;

            if (name_or_data != IntPtr.Zero)
            {
                int offset;
                int length;

                if ((mode & (MODE.OPENMEMORY | MODE.OPENMEMORY_POINT)) != 0)
                {
                    // OPENMEMORY_POINT won't work, so force it to OPENMEMORY
                    publicInfo.mode = (publicInfo.mode & ~MODE.OPENMEMORY_POINT) | MODE.OPENMEMORY;

                    // We want the data from (name_or_data + offset) to (name_or_data + offset + length)
                    offset = (int)exinfo.fileoffset;

                    // We'll copy the data taking fileoffset into account, so reset it to 0
                    publicInfo.exinfo.fileoffset = 0;

                    length = (int)exinfo.length;
                }
                else
                {
                    offset = 0;
                    length = MarshallingHelper.stringLengthUtf8(name_or_data) + 1;
                }

                publicInfo.name_or_data = new byte[length];
                Marshal.Copy(new IntPtr(name_or_data.ToInt64() + offset), publicInfo.name_or_data, 0, length);
            }
            else
            {
                publicInfo.name_or_data = null;
            }
        }
    }

    #endregion

    public enum USER_PROPERTY_TYPE
    {
        INTEGER,         /* Integer property */
        BOOLEAN,         /* Boolean property */
        FLOAT,           /* Float property */
        STRING,          /* String property */
    }

    public struct USER_PROPERTY
    {
        public string name;                /* Name of the user property. */
        public USER_PROPERTY_TYPE type;    /* Type of the user property. Use this to select one of the following values. */

        public int intValue;               /* Value of the user property. Only valid when type is USER_PROPERTY_TYPE.INTEGER. */
        public bool boolValue;             /* Value of the user property. Only valid when type is USER_PROPERTY_TYPE.BOOLEAN. */
        public float floatValue;           /* Value of the user property. Only valid when type is USER_PROPERTY_TYPE.FLOAT. */
        public string stringValue;         /* Value of the user property. Only valid when type is USER_PROPERTY_TYPE.STRING. */
    };

    #region wrapperinternal

    // The above structure has issues with strings and unions so we use this special marshalling struct instead
    [StructLayout(LayoutKind.Sequential)]
    struct USER_PROPERTY_INTERNAL
    {
        IntPtr name;                /* Name of the user property. */
        USER_PROPERTY_TYPE type;    /* Type of the user property. Use this to select one of the following values. */

        Union_IntBoolFloatString value;

        // Helper functions
        public USER_PROPERTY createPublic()
        {
            USER_PROPERTY publicProperty = new USER_PROPERTY();
            publicProperty.name = MarshallingHelper.stringFromNativeUtf8(name);
            publicProperty.type = type;

            switch (type)
            {
                case USER_PROPERTY_TYPE.INTEGER:
                    publicProperty.intValue = value.intValue;
                    break;
                case USER_PROPERTY_TYPE.BOOLEAN:
                    publicProperty.boolValue = value.boolValue;
                    break;
                case USER_PROPERTY_TYPE.FLOAT:
                    publicProperty.floatValue = value.floatValue;
                    break;
                case USER_PROPERTY_TYPE.STRING:
                    publicProperty.stringValue = MarshallingHelper.stringFromNativeUtf8(value.stringValue);
                    break;
            }

            return publicProperty;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct COMMAND_INFO_INTERNAL
    {
        public IntPtr commandName;                                         /* The full name of the API function for this command. */
        public int parentCommandIndex;                                     /* For commands that operate on an instance, this is the command that created the instance */
        public int frameNumber;                                            /* The frame the command belongs to */
        public float frameTime;                                            /* The playback time at which this command will be executed */
        public INSTANCETYPE instanceType;                                  /* The type of object that this command uses as an instance */
        public INSTANCETYPE outputType;                                    /* The type of object that this command outputs, if any */
        public UInt32 instanceHandle;                                      /* The original handle value of the instance.  This will no longer correspond to any actual object in playback. */
        public UInt32 outputHandle;                                        /* The original handle value of the command output.  This will no longer correspond to any actual object in playback. */

        // Helper functions
        public COMMAND_INFO createPublic()
        {
            COMMAND_INFO publicInfo = new COMMAND_INFO();
            publicInfo.commandName = MarshallingHelper.stringFromNativeUtf8(commandName);
            publicInfo.parentCommandIndex = parentCommandIndex;
            publicInfo.frameNumber = frameNumber;
            publicInfo.frameTime = frameTime;
            publicInfo.instanceType = instanceType;
            publicInfo.outputType = outputType;
            publicInfo.instanceHandle = instanceHandle;
            publicInfo.outputHandle = outputHandle;
            return publicInfo;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Union_IntBoolFloatString
    {
        [FieldOffset(0)]
        public int intValue;
        [FieldOffset(0)]
        public bool boolValue;
        [FieldOffset(0)]
        public float floatValue;
        [FieldOffset(0)]
        public IntPtr stringValue;
    }

    #endregion

    [Flags]
    public enum INITFLAGS : uint
    {
        NORMAL                  = 0x00000000,   /* Initialize normally. */
        LIVEUPDATE              = 0x00000001,   /* Enable live update. */
        ALLOW_MISSING_PLUGINS   = 0x00000002,   /* Load banks even if they reference plugins that have not been loaded. */
        SYNCHRONOUS_UPDATE      = 0x00000004,   /* Disable asynchronous processing and perform all processing on the calling thread instead. */
        DEFERRED_CALLBACKS      = 0x00000008,   /* Defer timeline callbacks until the main update. See Studio::EventInstance::setCallback for more information. */
    }

    [Flags]
    public enum LOAD_BANK_FLAGS : uint
    {
        NORMAL                  = 0x00000000,   /* Standard behaviour. */
        NONBLOCKING             = 0x00000001,   /* Bank loading occurs asynchronously rather than occurring immediately. */
        DECOMPRESS_SAMPLES      = 0x00000002,   /* Force samples to decompress into memory when they are loaded, rather than staying compressed. */
    }

    [Flags]
    public enum COMMANDCAPTURE_FLAGS : uint
    {
        NORMAL                  = 0x00000000,   /* Standard behaviour. */
        FILEFLUSH               = 0x00000001,   /* Call file flush on every command. */
        SKIP_INITIAL_STATE      = 0x00000002,   /* Normally the initial state of banks and instances is captured, unless this flag is set. */
    }

    [Flags]
    public enum COMMANDREPLAY_FLAGS : uint
    {
        NORMAL                  = 0x00000000,   /* Standard behaviour. */
        SKIP_CLEANUP            = 0x00000001,   /* Normally the playback will release any created resources when it stops, unless this flag is set. */
    }

    public enum PLAYBACK_STATE
    {
        PLAYING,               /* Currently playing. */
        SUSTAINING,            /* The timeline cursor is paused on a sustain point. */
        STOPPED,               /* Not playing. */
        STARTING,              /* Start has been called but the instance is not fully started yet. */
        STOPPING,              /* Stop has been called but the instance is not fully stopped yet. */
    }

    public enum EVENT_PROPERTY
    {
        CHANNELPRIORITY,        /* Priority to set on low-level channels created by this event instance (-1 to 256). */
        SCHEDULE_DELAY,         /* Schedule delay to synchronized playback for multiple tracks in DS clocks, or -1 for default. */
        SCHEDULE_LOOKAHEAD,     /* Schedule look-ahead on the timeline in DSP clocks, or -1 for default. */
        MINIMUM_DISTANCE,       /* Override the event's 3D minimum distance, or -1 for default. */
        MAXIMUM_DISTANCE        /* Override the event's 3D maximum distance, or -1 for default. */
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PLUGIN_INSTANCE_PROPERTIES
    {
        public IntPtr name;                           /* The name of the plugin effect or sound (set in FMOD Studio). */
        public IntPtr dsp;                            /* The DSP plugin instance. This can be cast to/from FMOD::DSP* type. */
    }

    [Flags]
    public enum EVENT_CALLBACK_TYPE : uint
    {
        CREATED                  = 0x00000001,  /* Called when an instance is fully created. Parameters = unused. */
        DESTROYED                = 0x00000002,  /* Called when an instance is just about to be destroyed. Parameters = unused. */
        STARTING                 = 0x00000004,  /* Called when an instance is preparing to start. Parameters = unused. */
        STARTED                  = 0x00000008,  /* Called when an instance starts playing. Parameters = unused. */
        RESTARTED                = 0x00000010,  /* Called when an instance is restarted. Parameters = unused. */
        STOPPED                  = 0x00000020,  /* Called when an instance stops. Parameters = unused. */
        START_FAILED             = 0x00000040,  /* Called when an instance did not start, e.g. due to polyphony. Parameters = unused. */
        CREATE_PROGRAMMER_SOUND  = 0x00000080,  /* Called when a programmer sound needs to be created in order to play a programmer instrument. Parameters = FMOD_STUDIO_PROGRAMMER_SOUND_PROPERTIES. */
        DESTROY_PROGRAMMER_SOUND = 0x00000100,  /* Called when a programmer sound needs to be destroyed. Parameters = FMOD_STUDIO_PROGRAMMER_SOUND_PROPERTIES. */
        PLUGIN_CREATED           = 0x00000200,  /* Called when a DSP plugin instance has just been created. Parameters = FMOD_STUDIO_PLUGIN_INSTANCE_PROPERTIES. */
        PLUGIN_DESTROYED         = 0x00000400,  /* Called when a DSP plugin instance is about to be destroyed. Parameters = FMOD_STUDIO_PLUGIN_INSTANCE_PROPERTIES. */
        TIMELINE_MARKER          = 0x00000800,  /* Called when the timeline passes a named marker.  Parameters = FMOD_STUDIO_TIMELINE_MARKER_PROPERTIES. */
        TIMELINE_BEAT            = 0x00001000,  /* Called when the timeline hits a beat in a tempo section.  Parameters = FMOD_STUDIO_TIMELINE_BEAT_PROPERTIES. */
        SOUND_PLAYED             = 0x00002000,  /* Called when the event plays a sound.  Parameters = FMOD::Sound. */
        SOUND_STOPPED            = 0x00004000,  /* Called when the event finishes playing a sound.  Parameters = FMOD::Sound. */

        ALL                      = 0xFFFFFFFF,  /* Pass this mask to Studio::EventDescription::setCallback or Studio::EventInstance::setCallback to receive all callback types. */
    }

    public delegate RESULT EVENT_CALLBACK(EVENT_CALLBACK_TYPE type, IntPtr eventInstance, IntPtr parameters);

    public delegate RESULT COMMANDREPLAY_FRAME_CALLBACK(IntPtr replay, int commandIndex, float currentTime, IntPtr userdata);
	public delegate RESULT COMMANDREPLAY_LOAD_BANK_CALLBACK(IntPtr replay, IntPtr guid, StringWrapper bankFilename, LOAD_BANK_FLAGS flags, out IntPtr bank, IntPtr userdata);
    public delegate RESULT COMMANDREPLAY_CREATE_INSTANCE_CALLBACK(IntPtr replay, IntPtr eventDescription, IntPtr originalHandle, out IntPtr instance, IntPtr userdata);

    public enum INSTANCETYPE
    {
        NONE,
        SYSTEM,
        EVENTDESCRIPTION,
        EVENTINSTANCE,
        PARAMETERINSTANCE,
        BUS,
        VCA,
        BANK,
        COMMANDREPLAY,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COMMAND_INFO
    {
        public string commandName;                                         /* The full name of the API function for this command. */
        public int parentCommandIndex;                                     /* For commands that operate on an instance, this is the command that created the instance */
        public int frameNumber;                                            /* The frame the command belongs to */
        public float frameTime;                                            /* The playback time at which this command will be executed */
        public INSTANCETYPE instanceType;                                  /* The type of object that this command uses as an instance */
        public INSTANCETYPE outputType;                                    /* The type of object that this command outputs, if any */
        public UInt32 instanceHandle;                                      /* The original handle value of the instance.  This will no longer correspond to any actual object in playback. */
        public UInt32 outputHandle;                                        /* The original handle value of the command output.  This will no longer correspond to any actual object in playback. */
    }

    public class Util
    {
        public static RESULT ParseID(string idString, out Guid id)
        {
			byte[] rawguid = new byte[16];
			RESULT result = FMOD_Studio_ParseID(Encoding.UTF8.GetBytes(idString + Char.MinValue), rawguid);
			id = new Guid(rawguid);
			return result;
        }

        #region importfunctions
        [DllImport (STUDIO_VERSION.dll)]
		private static extern RESULT FMOD_Studio_ParseID                      (byte[] idString, byte[] id);
        #endregion
    }

    public abstract class HandleBase
    {
        public HandleBase(IntPtr newPtr)
        {
            rawPtr = newPtr;
        }

        public bool isValid()
        {
            return (rawPtr != IntPtr.Zero) && isValidInternal();
        }

        protected abstract bool isValidInternal();

        public IntPtr getRaw()
        {
            return rawPtr;
        }

        protected IntPtr rawPtr;

        #region equality

        public override bool Equals(Object obj)
        {
            return Equals(obj as HandleBase);
        }
        public bool Equals(HandleBase p)
        {
            // Equals if p not null and handle is the same
            return ((object)p != null && rawPtr == p.rawPtr);
        }
        public override int GetHashCode()
        {
            return rawPtr.ToInt32();
        }
        public static bool operator ==(HandleBase a, HandleBase b)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            // Return true if the handle matches
            return (a.rawPtr == b.rawPtr);
        }
        public static bool operator !=(HandleBase a, HandleBase b)
        {
            return !(a == b);
        }
        #endregion

    }

    public class System : HandleBase
    {
        // Initialization / system functions.
        public static RESULT create(out System studiosystem)
        {
            RESULT      result           = RESULT.OK;
            IntPtr      rawPtr;
            studiosystem                 = null;

            result = FMOD_Studio_System_Create(out rawPtr, VERSION.number);
            if (result != RESULT.OK)
            {
                return result;
            }

            studiosystem = new System(rawPtr);

            return result;
        }
        public RESULT setAdvancedSettings(ADVANCEDSETTINGS settings)
        {
            #if NETFX_CORE
            settings.cbSize = Marshal.SizeOf<ADVANCEDSETTINGS>();
            #else
            settings.cbSize = Marshal.SizeOf(typeof(ADVANCEDSETTINGS));
            #endif
            return FMOD_Studio_System_SetAdvancedSettings(rawPtr, ref settings);
        }
        public RESULT getAdvancedSettings(out ADVANCEDSETTINGS settings)
        {
            #if NETFX_CORE
            settings.cbSize = Marshal.SizeOf<ADVANCEDSETTINGS>();
            #else
            settings.cbSize = Marshal.SizeOf(typeof(ADVANCEDSETTINGS));
            #endif
            return FMOD_Studio_System_GetAdvancedSettings(rawPtr, out settings);
        }
        public RESULT initialize(int maxchannels, INITFLAGS studioFlags, FMOD.INITFLAGS flags, IntPtr extradriverdata)
        {
            return FMOD_Studio_System_Initialize(rawPtr, maxchannels, studioFlags, flags, extradriverdata);
        }
        public RESULT release()
        {
            return FMOD_Studio_System_Release(rawPtr);
        }
        public RESULT update()
        {
            return FMOD_Studio_System_Update(rawPtr);
        }
        public RESULT getLowLevelSystem(out FMOD.System system)
        {
            system = null;

            IntPtr systemraw = new IntPtr();
            RESULT result = FMOD_Studio_System_GetLowLevelSystem(rawPtr, out systemraw);
            if (result != RESULT.OK)
            {
                return result;
            }

            system = new FMOD.System(systemraw);

            return result;
        }
        public RESULT getEvent(string path, out EventDescription _event)
        {
            _event = null;

            IntPtr eventraw = new IntPtr();
            RESULT result = FMOD_Studio_System_GetEvent(rawPtr, Encoding.UTF8.GetBytes(path + Char.MinValue), out eventraw);
            if (result != RESULT.OK)
            {
                return result;
            }

            _event = new EventDescription(eventraw);
            return result;
        }
        public RESULT getBus(string path, out Bus bus)
        {
            bus = null;

            IntPtr newPtr = new IntPtr();
            RESULT result = FMOD_Studio_System_GetBus(rawPtr, Encoding.UTF8.GetBytes(path + Char.MinValue), out newPtr);
            if (result != RESULT.OK)
            {
                return result;
            }

            bus = new Bus(newPtr);
            return result;
        }
        public RESULT getVCA(string path, out VCA vca)
        {
            vca = null;

            IntPtr newPtr = new IntPtr();
            RESULT result = FMOD_Studio_System_GetVCA(rawPtr, Encoding.UTF8.GetBytes(path + Char.MinValue), out newPtr);
            if (result != RESULT.OK)
            {
                return result;
            }

            vca = new VCA(newPtr);
            return result;
        }
        public RESULT getBank(string path, out Bank bank)
        {
            bank = null;

            IntPtr newPtr = new IntPtr();
            RESULT result = FMOD_Studio_System_GetBank(rawPtr, Encoding.UTF8.GetBytes(path + Char.MinValue), out newPtr);
            if (result != RESULT.OK)
            {
                return result;
            }

            bank = new Bank(newPtr);
            return result;
        }

        public RESULT getEventByID(Guid guid, out EventDescription _event)
        {
            _event = null;

            IntPtr eventraw = new IntPtr();
			RESULT result = FMOD_Studio_System_GetEventByID(rawPtr, guid.ToByteArray(), out eventraw);
            if (result != RESULT.OK)
            {
                return result;
            }

            _event = new EventDescription(eventraw);
            return result;
        }
        public RESULT getBusByID(Guid guid, out Bus bus)
        {
            bus = null;

            IntPtr newPtr = new IntPtr();
			RESULT result = FMOD_Studio_System_GetBusByID(rawPtr, guid.ToByteArray(), out newPtr);
            if (result != RESULT.OK)
            {
                return result;
            }

            bus = new Bus(newPtr);
            return result;
        }
        public RESULT getVCAByID(Guid guid, out VCA vca)
        {
            vca = null;

            IntPtr newPtr = new IntPtr();
			RESULT result = FMOD_Studio_System_GetVCAByID(rawPtr, guid.ToByteArray(), out newPtr);
            if (result != RESULT.OK)
            {
                return result;
            }

            vca = new VCA(newPtr);
            return result;
        }
        public RESULT getBankByID(Guid guid, out Bank bank)
        {
            bank = null;

            IntPtr newPtr = new IntPtr();
			RESULT result = FMOD_Studio_System_GetBankByID(rawPtr, guid.ToByteArray(), out newPtr);
            if (result != RESULT.OK)
            {
                return result;
            }

            bank = new Bank(newPtr);
            return result;
        }
        public RESULT getSoundInfo(string key, out SOUND_INFO info)
        {
            var size = Marshal.SizeOf(typeof(SOUND_INFO_INTERNAL));
            IntPtr infoPtr = Marshal.AllocHGlobal(size);
            
            RESULT result = FMOD_Studio_System_GetSoundInfo(rawPtr, Encoding.UTF8.GetBytes(key + Char.MinValue), infoPtr);
            if (result != RESULT.OK)
            {
                Marshal.FreeHGlobal(infoPtr);
                info = new SOUND_INFO();
                return result;
            }
            SOUND_INFO_INTERNAL internalInfo = (SOUND_INFO_INTERNAL)Marshal.PtrToStructure(infoPtr, typeof(SOUND_INFO_INTERNAL));
            internalInfo.assign(out info);
            Marshal.FreeHGlobal(infoPtr);

            return result;
        }
        public RESULT lookupID(string path, out Guid guid)
        {
			byte[] rawguid = new byte[16];
			RESULT result =  FMOD_Studio_System_LookupID(rawPtr, Encoding.UTF8.GetBytes(path + Char.MinValue), rawguid);
			guid = new Guid(rawguid);
			return result;
        }
        public RESULT lookupPath(Guid guid, out string path)
        {
            path = null;

            byte[] buffer = new byte[256];
            int retrieved = 0;
			RESULT result = FMOD_Studio_System_LookupPath(rawPtr, guid.ToByteArray(), buffer, buffer.Length, out retrieved);

            if (result == RESULT.ERR_TRUNCATED)
            {
                buffer = new byte[retrieved];
				result = FMOD_Studio_System_LookupPath(rawPtr, guid.ToByteArray(), buffer, buffer.Length, out retrieved);
            }

            if (result == RESULT.OK)
            {
                path = Encoding.UTF8.GetString(buffer, 0, retrieved - 1);
            }

            return result;
        }
        public RESULT getNumListeners(out int numlisteners)
        {
            return FMOD_Studio_System_GetNumListeners(rawPtr, out numlisteners);
        }
        public RESULT setNumListeners(int numlisteners)
        {
            return FMOD_Studio_System_SetNumListeners(rawPtr, numlisteners);
        }
        public RESULT getListenerAttributes(int listener, out ATTRIBUTES_3D attributes)
        {
            return FMOD_Studio_System_GetListenerAttributes(rawPtr, listener, out attributes);
        }
        public RESULT setListenerAttributes(int listener, ATTRIBUTES_3D attributes)
        {
            return FMOD_Studio_System_SetListenerAttributes(rawPtr, listener, ref attributes);
        }
        public RESULT loadBankFile(string name, LOAD_BANK_FLAGS flags, out Bank bank)
        {
            bank = null;

            IntPtr newPtr = new IntPtr();
            RESULT result = FMOD_Studio_System_LoadBankFile(rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), flags, out newPtr);
            if (result != RESULT.OK)
            {
                return result;
            }

            bank = new Bank(newPtr);
            return result;
        }
        public RESULT loadBankMemory(byte[] buffer, LOAD_BANK_FLAGS flags, out Bank bank)
        {
            bank = null;

            IntPtr newPtr = new IntPtr();

            // Manually pin the byte array. It's what the marshaller should do anyway but don't leave it to chance.
            GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
            RESULT result = FMOD_Studio_System_LoadBankMemory(rawPtr, pointer, buffer.Length, LOAD_MEMORY_MODE.LOAD_MEMORY, flags, out newPtr);
            pinnedArray.Free();
            if (result != RESULT.OK)
            {
                return result;
            }

            bank = new Bank(newPtr);
            return result;
        }
        public RESULT loadBankCustom(BANK_INFO info, LOAD_BANK_FLAGS flags, out Bank bank)
        {
            bank = null;

            info.size = Marshal.SizeOf(info);

            IntPtr newPtr = new IntPtr();
            RESULT result = FMOD_Studio_System_LoadBankCustom(rawPtr, ref info, flags, out newPtr);
            if (result != RESULT.OK)
            {
                return result;
            }

            bank = new Bank(newPtr);
            return result;
        }
        public RESULT unloadAll()
        {
            return FMOD_Studio_System_UnloadAll(rawPtr);
        }
        public RESULT flushCommands()
        {
            return FMOD_Studio_System_FlushCommands(rawPtr);
        }
        public RESULT flushSampleLoading()
        {
            return FMOD_Studio_System_FlushSampleLoading(rawPtr);
        }
        public RESULT startCommandCapture(string path, COMMANDCAPTURE_FLAGS flags)
        {
            return FMOD_Studio_System_StartCommandCapture(rawPtr, Encoding.UTF8.GetBytes(path + Char.MinValue), flags);
        }
        public RESULT stopCommandCapture()
        {
            return FMOD_Studio_System_StopCommandCapture(rawPtr);
        }
        public RESULT loadCommandReplay(string path, COMMANDREPLAY_FLAGS flags, out CommandReplay replay)
        {
            replay = null;
            IntPtr newPtr = new IntPtr();
            RESULT result = FMOD_Studio_System_LoadCommandReplay(rawPtr, Encoding.UTF8.GetBytes(path + Char.MinValue), flags, out newPtr);
            if (result == RESULT.OK)
            {
                replay = new CommandReplay(newPtr);
            }
            return result;
        }
        public RESULT getBankCount(out int count)
        {
            return FMOD_Studio_System_GetBankCount(rawPtr, out count);
        }
        public RESULT getBankList(out Bank[] array)
        {
            array = null;

            RESULT result;
            int capacity;
            result = FMOD_Studio_System_GetBankCount(rawPtr, out capacity);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (capacity == 0)
            {
                array = new Bank[0];
                return result;
            }

            IntPtr[] rawArray = new IntPtr[capacity];
            int actualCount;
            result = FMOD_Studio_System_GetBankList(rawPtr, rawArray, capacity, out actualCount);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (actualCount > capacity) // More items added since we queried just now?
            {
                actualCount = capacity;
            }
            array = new Bank[actualCount];
            for (int i=0; i<actualCount; ++i)
            {
                array[i] = new Bank(rawArray[i]);
            }
            return RESULT.OK;
        }
        public RESULT getCPUUsage(out CPU_USAGE usage)
        {
            return FMOD_Studio_System_GetCPUUsage(rawPtr, out usage);
        }
        public RESULT getBufferUsage(out BUFFER_USAGE usage)
        {
            return FMOD_Studio_System_GetBufferUsage(rawPtr, out usage);
        }
        public RESULT resetBufferUsage()
        {
            return FMOD_Studio_System_ResetBufferUsage(rawPtr);
        }

        public RESULT setCallback(SYSTEM_CALLBACK callback, SYSTEM_CALLBACK_TYPE callbackmask = SYSTEM_CALLBACK_TYPE.ALL)
        {
            return FMOD_Studio_System_SetCallback(rawPtr, callback, callbackmask);
        }

        public RESULT getUserData(out IntPtr userData)
        {
            return FMOD_Studio_System_GetUserData(rawPtr, out userData);
        }

        public RESULT setUserData(IntPtr userData)
        {
            return FMOD_Studio_System_SetUserData(rawPtr, userData);
        }

        #region importfunctions
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_Create                  (out IntPtr studiosystem, uint headerversion);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern bool   FMOD_Studio_System_IsValid                 (IntPtr studiosystem);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_SetAdvancedSettings     (IntPtr studiosystem, ref ADVANCEDSETTINGS settings);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetAdvancedSettings     (IntPtr studiosystem, out ADVANCEDSETTINGS settings);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_Initialize              (IntPtr studiosystem, int maxchannels, INITFLAGS studioFlags, FMOD.INITFLAGS flags, IntPtr extradriverdata);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_Release                 (IntPtr studiosystem);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_Update                  (IntPtr studiosystem);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetLowLevelSystem       (IntPtr studiosystem, out IntPtr system);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetEvent                (IntPtr studiosystem, byte[] path, out IntPtr description);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetBus                  (IntPtr studiosystem, byte[] path, out IntPtr bus);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetVCA                  (IntPtr studiosystem, byte[] path, out IntPtr vca);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetBank                 (IntPtr studiosystem, byte[] path, out IntPtr bank);
        [DllImport (STUDIO_VERSION.dll)]
		private static extern RESULT FMOD_Studio_System_GetEventByID            (IntPtr studiosystem, byte[] guid, out IntPtr description);
        [DllImport (STUDIO_VERSION.dll)]
		private static extern RESULT FMOD_Studio_System_GetBusByID              (IntPtr studiosystem, byte[] guid, out IntPtr bus);
        [DllImport (STUDIO_VERSION.dll)]
		private static extern RESULT FMOD_Studio_System_GetVCAByID              (IntPtr studiosystem, byte[] guid, out IntPtr vca);
        [DllImport (STUDIO_VERSION.dll)]
		private static extern RESULT FMOD_Studio_System_GetBankByID             (IntPtr studiosystem, byte[] guid, out IntPtr bank);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetSoundInfo            (IntPtr studiosystem, byte[] key, IntPtr info);
        [DllImport (STUDIO_VERSION.dll)]
		private static extern RESULT FMOD_Studio_System_LookupID                (IntPtr studiosystem, byte[] path, [Out] byte[] guid);
        [DllImport(STUDIO_VERSION.dll)]
		private static extern RESULT FMOD_Studio_System_LookupPath              (IntPtr studiosystem, byte[] guid, [Out] byte[] path, int size, out int retrieved);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetNumListeners         (IntPtr studiosystem, out int numlisteners);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_SetNumListeners         (IntPtr studiosystem, int numlisteners);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetListenerAttributes   (IntPtr studiosystem, int listener, out ATTRIBUTES_3D attributes);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_SetListenerAttributes   (IntPtr studiosystem, int listener, ref ATTRIBUTES_3D attributes);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_LoadBankFile            (IntPtr studiosystem, byte[] filename, LOAD_BANK_FLAGS flags, out IntPtr bank);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_LoadBankMemory(IntPtr studiosystem, IntPtr buffer, int length, LOAD_MEMORY_MODE mode, LOAD_BANK_FLAGS flags, out IntPtr bank);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_LoadBankCustom          (IntPtr studiosystem, ref BANK_INFO info, LOAD_BANK_FLAGS flags, out IntPtr bank);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_UnloadAll               (IntPtr studiosystem);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_FlushCommands           (IntPtr studiosystem);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_FlushSampleLoading      (IntPtr studiosystem);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_StartCommandCapture     (IntPtr studiosystem, byte[] path, COMMANDCAPTURE_FLAGS flags);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_StopCommandCapture      (IntPtr studiosystem);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_LoadCommandReplay       (IntPtr studiosystem, byte[] path, COMMANDREPLAY_FLAGS flags, out IntPtr commandReplay);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetBankCount            (IntPtr studiosystem, out int count);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetBankList             (IntPtr studiosystem, IntPtr[] array, int capacity, out int count);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetCPUUsage             (IntPtr studiosystem, out CPU_USAGE usage);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetBufferUsage          (IntPtr studiosystem, out BUFFER_USAGE usage);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_ResetBufferUsage        (IntPtr studiosystem);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_SetCallback             (IntPtr studiosystem, SYSTEM_CALLBACK callback, SYSTEM_CALLBACK_TYPE callbackmask);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_GetUserData             (IntPtr studiosystem, out IntPtr userData);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_SetUserData             (IntPtr studiosystem, IntPtr userData);
        #endregion

        #region wrapperinternal

        public System(IntPtr raw)
        : base(raw)
        {
        }

        protected override bool isValidInternal()
        {
            return FMOD_Studio_System_IsValid(rawPtr);
        }

        #endregion
    }

    public class EventDescription : HandleBase
    {
        public RESULT getID(out Guid id)
        {
			byte[] rawguid = new byte[16];
			RESULT result = FMOD_Studio_EventDescription_GetID(rawPtr, rawguid);
			id = new Guid(rawguid);
			return result;
        }
        public RESULT getPath(out string path)
        {
            path = null;

            byte[] buffer = new byte[256];
            int retrieved = 0;
            RESULT result = FMOD_Studio_EventDescription_GetPath(rawPtr, buffer, buffer.Length, out retrieved);

            if (result == RESULT.ERR_TRUNCATED)
            {
                buffer = new byte[retrieved];
                result = FMOD_Studio_EventDescription_GetPath(rawPtr, buffer, buffer.Length, out retrieved);
            }

            if (result == RESULT.OK)
            {
                path = Encoding.UTF8.GetString(buffer, 0, retrieved - 1);
            }

            return result;
        }
        public RESULT getParameterCount(out int count)
        {
            return FMOD_Studio_EventDescription_GetParameterCount(rawPtr, out count);
        }
        public RESULT getParameterByIndex(int index, out PARAMETER_DESCRIPTION parameter)
        {
            parameter = new PARAMETER_DESCRIPTION();

            PARAMETER_DESCRIPTION_INTERNAL paramInternal;
            RESULT result = FMOD_Studio_EventDescription_GetParameterByIndex(rawPtr, index, out paramInternal);
            if (result != RESULT.OK)
            {
                return result;
            }
            paramInternal.assign(out parameter);
            return result;
        }
        public RESULT getParameter(string name, out PARAMETER_DESCRIPTION parameter)
        {
            parameter = new PARAMETER_DESCRIPTION();

            PARAMETER_DESCRIPTION_INTERNAL paramInternal;
            RESULT result = FMOD_Studio_EventDescription_GetParameter(rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), out paramInternal);
            if (result != RESULT.OK)
            {
                return result;
            }
            paramInternal.assign(out parameter);
            return result;
        }
        public RESULT getUserPropertyCount(out int count)
        {
            return FMOD_Studio_EventDescription_GetUserPropertyCount(rawPtr, out count);
        }
        public RESULT getUserPropertyByIndex(int index, out USER_PROPERTY property)
        {
            USER_PROPERTY_INTERNAL propertyInternal;

            RESULT result = FMOD_Studio_EventDescription_GetUserPropertyByIndex(rawPtr, index, out propertyInternal);
            if (result != RESULT.OK)
            {
                property = new USER_PROPERTY();
                return result;
            }

            property = propertyInternal.createPublic();

            return RESULT.OK;
        }
        public RESULT getUserProperty(string name, out USER_PROPERTY property)
        {
            USER_PROPERTY_INTERNAL propertyInternal;

            RESULT result = FMOD_Studio_EventDescription_GetUserProperty(
                rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), out propertyInternal);
            if (result != RESULT.OK)
            {
                property = new USER_PROPERTY();
                return result;
            }

            property = propertyInternal.createPublic();

            return RESULT.OK;
        }
        public RESULT getLength(out int length)
        {
            return FMOD_Studio_EventDescription_GetLength(rawPtr, out length);
        }
        public RESULT getMinimumDistance(out float distance)
        {
            return FMOD_Studio_EventDescription_GetMinimumDistance(rawPtr, out distance);
        }
        public RESULT getMaximumDistance(out float distance)
        {
            return FMOD_Studio_EventDescription_GetMaximumDistance(rawPtr, out distance);
        }
        public RESULT getSoundSize(out float size)
        {
            return FMOD_Studio_EventDescription_GetSoundSize(rawPtr, out size);
        }
        public RESULT isOneshot(out bool oneshot)
        {
            return FMOD_Studio_EventDescription_IsOneshot(rawPtr, out oneshot);
        }
        public RESULT isStream(out bool isStream)
        {
            return FMOD_Studio_EventDescription_IsStream(rawPtr, out isStream);
        }
        public RESULT is3D(out bool is3D)
        {
            return FMOD_Studio_EventDescription_Is3D(rawPtr, out is3D);
        }
        public RESULT hasCue(out bool cue)
        {
            return FMOD_Studio_EventDescription_HasCue(rawPtr, out cue);
        }

        public RESULT createInstance(out EventInstance instance)
        {
            instance = null;

            IntPtr newPtr = new IntPtr();
            RESULT result = FMOD_Studio_EventDescription_CreateInstance(rawPtr, out newPtr);
            if (result != RESULT.OK)
            {
                return result;
            }
            instance = new EventInstance(newPtr);
            return result;
        }

        public RESULT getInstanceCount(out int count)
        {
            return FMOD_Studio_EventDescription_GetInstanceCount(rawPtr, out count);
        }
        public RESULT getInstanceList(out EventInstance[] array)
        {
            array = null;

            RESULT result;
            int capacity;
            result = FMOD_Studio_EventDescription_GetInstanceCount(rawPtr, out capacity);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (capacity == 0)
            {
                array = new EventInstance[0];
                return result;
            }

            IntPtr[] rawArray = new IntPtr[capacity];
            int actualCount;
            result = FMOD_Studio_EventDescription_GetInstanceList(rawPtr, rawArray, capacity, out actualCount);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (actualCount > capacity) // More items added since we queried just now?
            {
                actualCount = capacity;
            }
            array = new EventInstance[actualCount];
            for (int i=0; i<actualCount; ++i)
            {
                array[i] = new EventInstance(rawArray[i]);
            }
            return RESULT.OK;
        }

        public RESULT loadSampleData()
        {
            return FMOD_Studio_EventDescription_LoadSampleData(rawPtr);
        }

        public RESULT unloadSampleData()
        {
            return FMOD_Studio_EventDescription_UnloadSampleData(rawPtr);
        }

        public RESULT getSampleLoadingState(out LOADING_STATE state)
        {
            return FMOD_Studio_EventDescription_GetSampleLoadingState(rawPtr, out state);
        }

        public RESULT releaseAllInstances()
        {
            return FMOD_Studio_EventDescription_ReleaseAllInstances(rawPtr);
        }
        public RESULT setCallback(EVENT_CALLBACK callback, EVENT_CALLBACK_TYPE callbackmask = EVENT_CALLBACK_TYPE.ALL)
        {
            return FMOD_Studio_EventDescription_SetCallback(rawPtr, callback, callbackmask);
        }

        public RESULT getUserData(out IntPtr userData)
        {
            return FMOD_Studio_EventDescription_GetUserData(rawPtr, out userData);
        }

        public RESULT setUserData(IntPtr userData)
        {
            return FMOD_Studio_EventDescription_SetUserData(rawPtr, userData);
        }

        #region importfunctions
        [DllImport(STUDIO_VERSION.dll)]
        private static extern bool FMOD_Studio_EventDescription_IsValid(IntPtr eventdescription);
        [DllImport (STUDIO_VERSION.dll)]
		private static extern RESULT FMOD_Studio_EventDescription_GetID(IntPtr eventdescription, [Out] byte[] id);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetPath(IntPtr eventdescription, [Out] byte[] path, int size, out int retrieved);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetParameterCount(IntPtr eventdescription, out int count);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetParameterByIndex(IntPtr eventdescription, int index, out PARAMETER_DESCRIPTION_INTERNAL parameter);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetParameter(IntPtr eventdescription, byte[] name, out PARAMETER_DESCRIPTION_INTERNAL parameter);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetUserPropertyCount(IntPtr eventdescription, out int count);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetUserPropertyByIndex(IntPtr eventdescription, int index, out USER_PROPERTY_INTERNAL property);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetUserProperty(IntPtr eventdescription, byte[] name, out USER_PROPERTY_INTERNAL property);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetLength(IntPtr eventdescription, out int length);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetMinimumDistance(IntPtr eventdescription, out float distance);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetMaximumDistance(IntPtr eventdescription, out float distance);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetSoundSize(IntPtr eventdescription, out float size);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_IsOneshot(IntPtr eventdescription, out bool oneshot);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_IsStream(IntPtr eventdescription, out bool isStream);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_Is3D(IntPtr eventdescription, out bool is3D);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_HasCue(IntPtr eventdescription, out bool cue);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_CreateInstance(IntPtr eventdescription, out IntPtr instance);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetInstanceCount(IntPtr eventdescription, out int count);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetInstanceList(IntPtr eventdescription, IntPtr[] array, int capacity, out int count);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_LoadSampleData(IntPtr eventdescription);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_UnloadSampleData(IntPtr eventdescription);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetSampleLoadingState(IntPtr eventdescription, out LOADING_STATE state);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_ReleaseAllInstances(IntPtr eventdescription);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_SetCallback(IntPtr eventdescription, EVENT_CALLBACK callback, EVENT_CALLBACK_TYPE callbackmask);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_GetUserData(IntPtr eventdescription, out IntPtr userData);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventDescription_SetUserData(IntPtr eventdescription, IntPtr userData);
        #endregion
        #region wrapperinternal

        public EventDescription(IntPtr raw)
        : base(raw)
        {
        }

        protected override bool isValidInternal()
        {
            return FMOD_Studio_EventDescription_IsValid(rawPtr);
        }

        #endregion
    }

    public class EventInstance : HandleBase
    {
        public RESULT getDescription(out EventDescription description)
        {
            description = null;

            IntPtr newPtr;
            RESULT result = FMOD_Studio_EventInstance_GetDescription(rawPtr, out newPtr);
            if (result != RESULT.OK)
            {
                return result;
            }
            description = new EventDescription(newPtr);
            return result;
        }
        public RESULT getVolume(out float volume)
        {
            return FMOD_Studio_EventInstance_GetVolume(rawPtr, out volume);
        }
        public RESULT setVolume(float volume)
        {
            return FMOD_Studio_EventInstance_SetVolume(rawPtr, volume);
        }
        public RESULT getPitch(out float pitch)
        {
            return FMOD_Studio_EventInstance_GetPitch(rawPtr, out pitch);
        }
        public RESULT setPitch(float pitch)
        {
            return FMOD_Studio_EventInstance_SetPitch(rawPtr, pitch);
        }
        public RESULT get3DAttributes(out ATTRIBUTES_3D attributes)
        {
            return FMOD_Studio_EventInstance_Get3DAttributes(rawPtr, out attributes);
        }
        public RESULT set3DAttributes               (ATTRIBUTES_3D attributes)
        {
            return FMOD_Studio_EventInstance_Set3DAttributes(rawPtr, ref attributes);
        }
        public RESULT getProperty(EVENT_PROPERTY index, out float value)
        {
            return FMOD_Studio_EventInstance_GetProperty(rawPtr, index, out value);
        }
        public RESULT setProperty(EVENT_PROPERTY index, float value)
        {
            return FMOD_Studio_EventInstance_SetProperty(rawPtr, index, value);
        }
        public RESULT getPaused(out bool paused)
        {
            return FMOD_Studio_EventInstance_GetPaused(rawPtr, out paused);
        }
        public RESULT setPaused(bool paused)
        {
            return FMOD_Studio_EventInstance_SetPaused(rawPtr, paused);
        }
        public RESULT start()
        {
            return FMOD_Studio_EventInstance_Start(rawPtr);
        }
        public RESULT stop(STOP_MODE mode)
        {
            return FMOD_Studio_EventInstance_Stop(rawPtr, mode);
        }
        public RESULT getTimelinePosition(out int position)
        {
            return FMOD_Studio_EventInstance_GetTimelinePosition(rawPtr, out position);
        }
        public RESULT setTimelinePosition(int position)
        {
            return FMOD_Studio_EventInstance_SetTimelinePosition(rawPtr, position);
        }
        public RESULT getPlaybackState(out PLAYBACK_STATE state)
        {
            return FMOD_Studio_EventInstance_GetPlaybackState(rawPtr, out state);
        }
        public RESULT getChannelGroup(out FMOD.ChannelGroup group)
        {
            group = null;

            IntPtr groupraw = new IntPtr();
            RESULT result = FMOD_Studio_EventInstance_GetChannelGroup(rawPtr, out groupraw);
            if (result != RESULT.OK)
            {
                return result;
            }

            group = new FMOD.ChannelGroup(groupraw);

            return result;
        }
        public RESULT release()
        {
            return FMOD_Studio_EventInstance_Release(rawPtr);
        }
        public RESULT isVirtual(out bool virtualState)
        {
            return FMOD_Studio_EventInstance_IsVirtual(rawPtr, out virtualState);
        }
        public RESULT getParameter(string name, out ParameterInstance instance)
        {
            instance = null;

            IntPtr newPtr = new IntPtr();
            RESULT result = FMOD_Studio_EventInstance_GetParameter(rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), out newPtr);
            if (result != RESULT.OK)
            {
                return result;
            }
            instance = new ParameterInstance(newPtr);

            return result;
        }
        public RESULT getParameterCount(out int count)
        {
            return FMOD_Studio_EventInstance_GetParameterCount(rawPtr, out count);
        }
        public RESULT getParameterByIndex(int index, out ParameterInstance instance)
        {
            instance = null;

            IntPtr newPtr = new IntPtr();
            RESULT result = FMOD_Studio_EventInstance_GetParameterByIndex(rawPtr, index, out newPtr);
            if (result != RESULT.OK)
            {
                return result;
            }
            instance = new ParameterInstance(newPtr);

            return result;
        }
        public RESULT getParameterValue(string name, out float value)
        {
            return FMOD_Studio_EventInstance_GetParameterValue(rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), out value);
        }
        public RESULT setParameterValue(string name, float value)
        {
            return FMOD_Studio_EventInstance_SetParameterValue(rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), value);
        }
        public RESULT getParameterValueByIndex(int index, out float value)
        {
            return FMOD_Studio_EventInstance_GetParameterValueByIndex(rawPtr, index, out value);
        }
        public RESULT setParameterValueByIndex(int index, float value)
        {
            return FMOD_Studio_EventInstance_SetParameterValueByIndex(rawPtr, index, value);
        }
        public RESULT triggerCue()
        {
            return FMOD_Studio_EventInstance_TriggerCue(rawPtr);
        }
        public RESULT setCallback(EVENT_CALLBACK callback, EVENT_CALLBACK_TYPE callbackmask = EVENT_CALLBACK_TYPE.ALL)
        {
            return FMOD_Studio_EventInstance_SetCallback(rawPtr, callback, callbackmask);
        }
        public RESULT getUserData(out IntPtr userData)
        {
            return FMOD_Studio_EventInstance_GetUserData(rawPtr, out userData);
        }
        public RESULT setUserData(IntPtr userData)
        {
            return FMOD_Studio_EventInstance_SetUserData(rawPtr, userData);
        }

        #region importfunctions
        [DllImport(STUDIO_VERSION.dll)]
        private static extern bool FMOD_Studio_EventInstance_IsValid(IntPtr _event);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetDescription       (IntPtr _event, out IntPtr description);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetVolume            (IntPtr _event, out float volume);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_SetVolume            (IntPtr _event, float volume);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetPitch             (IntPtr _event, out float pitch);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_SetPitch             (IntPtr _event, float pitch);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_Get3DAttributes      (IntPtr _event, out ATTRIBUTES_3D attributes);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_Set3DAttributes      (IntPtr _event, ref ATTRIBUTES_3D attributes);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetProperty          (IntPtr _event, EVENT_PROPERTY index, out float value);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_SetProperty          (IntPtr _event, EVENT_PROPERTY index, float value);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetPaused            (IntPtr _event, out bool paused);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_SetPaused            (IntPtr _event, bool paused);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_Start                (IntPtr _event);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_Stop                 (IntPtr _event, STOP_MODE mode);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetTimelinePosition  (IntPtr _event, out int position);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_SetTimelinePosition  (IntPtr _event, int position);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetPlaybackState     (IntPtr _event, out PLAYBACK_STATE state);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetChannelGroup      (IntPtr _event, out IntPtr group);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_Release              (IntPtr _event);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_IsVirtual            (IntPtr _event, out bool virtualState);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetParameter         (IntPtr _event, byte[] name, out IntPtr parameter);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetParameterByIndex  (IntPtr _event, int index, out IntPtr parameter);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetParameterCount    (IntPtr _event, out int count);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetParameterValue    (IntPtr _event, byte[] name, out float value);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_SetParameterValue    (IntPtr _event, byte[] name, float value);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetParameterValueByIndex (IntPtr _event, int index, out float value);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_SetParameterValueByIndex (IntPtr _event, int index, float value);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_TriggerCue           (IntPtr _event);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_SetCallback          (IntPtr _event, EVENT_CALLBACK callback, EVENT_CALLBACK_TYPE callbackmask);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_GetUserData          (IntPtr _event, out IntPtr userData);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_SetUserData          (IntPtr _event, IntPtr userData);
        #endregion

        #region wrapperinternal

        public EventInstance(IntPtr raw)
        : base(raw)
        {
        }

        protected override bool isValidInternal()
        {
            return FMOD_Studio_EventInstance_IsValid(rawPtr);
        }

        #endregion
    }

    public class ParameterInstance : HandleBase
    {
        public RESULT getDescription(out PARAMETER_DESCRIPTION description)
        {
            description = new PARAMETER_DESCRIPTION();

            PARAMETER_DESCRIPTION_INTERNAL paramInternal;
            RESULT result = FMOD_Studio_ParameterInstance_GetDescription(rawPtr, out paramInternal);
            if (result != RESULT.OK)
            {
                return result;
            }
            paramInternal.assign(out description);
            return result;
        }

        public RESULT getValue(out float value)
        {
            return FMOD_Studio_ParameterInstance_GetValue(rawPtr, out value);
        }
        public RESULT setValue(float value)
        {
            return FMOD_Studio_ParameterInstance_SetValue(rawPtr, value);
        }

        #region importfunctions
        [DllImport(STUDIO_VERSION.dll)]
        private static extern bool FMOD_Studio_ParameterInstance_IsValid(IntPtr parameter);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_ParameterInstance_GetDescription(IntPtr parameter, out PARAMETER_DESCRIPTION_INTERNAL description);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_ParameterInstance_GetValue(IntPtr parameter, out float value);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_ParameterInstance_SetValue(IntPtr parameter, float value);
        #endregion

        #region wrapperinternal

        public ParameterInstance(IntPtr raw)
        : base(raw)
        {
        }

        protected override bool isValidInternal()
        {
            return FMOD_Studio_ParameterInstance_IsValid(rawPtr);
        }

        #endregion
    }

    public class Bus : HandleBase
    {
        public RESULT getID(out Guid id)
        {
			byte[] rawguid = new byte[16];
			RESULT result = FMOD_Studio_Bus_GetID(rawPtr, rawguid);
			id = new Guid(rawguid);
			return result;
        }
        public RESULT getPath(out string path)
        {
            path = null;

            byte[] buffer = new byte[256];
            int retrieved = 0;
            RESULT result = FMOD_Studio_Bus_GetPath(rawPtr, buffer, buffer.Length, out retrieved);

            if (result == RESULT.ERR_TRUNCATED)
            {
                buffer = new byte[retrieved];
                result = FMOD_Studio_Bus_GetPath(rawPtr, buffer, buffer.Length, out retrieved);
            }

            if (result == RESULT.OK)
            {
                path = Encoding.UTF8.GetString(buffer, 0, retrieved - 1);
            }

            return result;
        }
        public RESULT getFaderLevel(out float volume)
        {
            return FMOD_Studio_Bus_GetFaderLevel(rawPtr, out volume);
        }
        public RESULT setFaderLevel(float volume)
        {
            return FMOD_Studio_Bus_SetFaderLevel(rawPtr, volume);
        }
        public RESULT getPaused(out bool paused)
        {
            return FMOD_Studio_Bus_GetPaused(rawPtr, out paused);
        }
        public RESULT setPaused(bool paused)
        {
            return FMOD_Studio_Bus_SetPaused(rawPtr, paused);
        }
        public RESULT getMute(out bool mute)
        {
            return FMOD_Studio_Bus_GetMute(rawPtr, out mute);
        }
        public RESULT setMute(bool mute)
        {
            return FMOD_Studio_Bus_SetMute(rawPtr, mute);
        }
        public RESULT stopAllEvents(STOP_MODE mode)
        {
            return FMOD_Studio_Bus_StopAllEvents(rawPtr, mode);
        }
        public RESULT lockChannelGroup()
        {
            return FMOD_Studio_Bus_LockChannelGroup(rawPtr);
        }
        public RESULT unlockChannelGroup()
        {
            return FMOD_Studio_Bus_UnlockChannelGroup(rawPtr);
        }
        public RESULT getChannelGroup(out FMOD.ChannelGroup group)
        {
            group = null;

            IntPtr groupraw = new IntPtr();
            RESULT result = FMOD_Studio_Bus_GetChannelGroup(rawPtr, out groupraw);
            if (result != RESULT.OK)
            {
                return result;
            }

            group = new FMOD.ChannelGroup(groupraw);

            return result;
        }

        #region importfunctions
        [DllImport(STUDIO_VERSION.dll)]
        private static extern bool FMOD_Studio_Bus_IsValid(IntPtr bus);
        [DllImport(STUDIO_VERSION.dll)]
		private static extern RESULT FMOD_Studio_Bus_GetID(IntPtr bus, [Out] byte[] id);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bus_GetPath(IntPtr bus, [Out] byte[] path, int size, out int retrieved);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bus_GetFaderLevel(IntPtr bus, out float value);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bus_SetFaderLevel(IntPtr bus, float value);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bus_GetPaused(IntPtr bus, out bool paused);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bus_SetPaused(IntPtr bus, bool paused);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bus_GetMute(IntPtr bus, out bool mute);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bus_SetMute(IntPtr bus, bool mute);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bus_StopAllEvents(IntPtr bus, STOP_MODE mode);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bus_LockChannelGroup(IntPtr bus);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bus_UnlockChannelGroup(IntPtr bus);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bus_GetChannelGroup(IntPtr bus, out IntPtr group);
        #endregion

        #region wrapperinternal

        public Bus(IntPtr raw)
        : base(raw)
        {
        }

        protected override bool isValidInternal()
        {
            return FMOD_Studio_Bus_IsValid(rawPtr);
        }

        #endregion
    }

    public class VCA : HandleBase
    {
        public RESULT getID(out Guid id)
        {
			byte[] rawguid = new byte[16];
			RESULT result = FMOD_Studio_VCA_GetID(rawPtr, rawguid);
			id = new Guid(rawguid);
			return result;
        }
        public RESULT getPath(out string path)
        {
            path = null;

            byte[] buffer = new byte[256];
            int retrieved = 0;
            RESULT result = FMOD_Studio_VCA_GetPath(rawPtr, buffer, buffer.Length, out retrieved);

            if (result == RESULT.ERR_TRUNCATED)
            {
                buffer = new byte[retrieved];
                result = FMOD_Studio_VCA_GetPath(rawPtr, buffer, buffer.Length, out retrieved);
            }

            if (result == RESULT.OK)
            {
                path = Encoding.UTF8.GetString(buffer, 0, retrieved - 1);
            }

            return result;
        }
        public RESULT getFaderLevel(out float volume)
        {
            return FMOD_Studio_VCA_GetFaderLevel(rawPtr, out volume);
        }
        public RESULT setFaderLevel(float volume)
        {
            return FMOD_Studio_VCA_SetFaderLevel(rawPtr, volume);
        }

        #region importfunctions
        [DllImport(STUDIO_VERSION.dll)]
        private static extern bool FMOD_Studio_VCA_IsValid(IntPtr vca);
        [DllImport(STUDIO_VERSION.dll)]
		private static extern RESULT FMOD_Studio_VCA_GetID(IntPtr vca, [Out] byte[] id);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_VCA_GetPath(IntPtr vca, [Out] byte[] path, int size, out int retrieved);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_VCA_GetFaderLevel(IntPtr vca, out float value);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_VCA_SetFaderLevel(IntPtr vca, float value);
        #endregion

        #region wrapperinternal

        public VCA(IntPtr raw)
            : base(raw)
        {
        }

        protected override bool isValidInternal()
        {
            return FMOD_Studio_VCA_IsValid(rawPtr);
        }

        #endregion
    }

    public class Bank : HandleBase
    {
        // Property access

        public RESULT getID(out Guid id)
        {
			byte[] rawguid = new byte[16];
			RESULT result = FMOD_Studio_Bank_GetID(rawPtr, rawguid);
			id = new Guid(rawguid);
			return result;
        }
        public RESULT getPath(out string path)
        {
            path = null;

            byte[] buffer = new byte[256];
            int retrieved = 0;
            RESULT result = FMOD_Studio_Bank_GetPath(rawPtr, buffer, buffer.Length, out retrieved);

            if (result == RESULT.ERR_TRUNCATED)
            {
                buffer = new byte[retrieved];
                result = FMOD_Studio_Bank_GetPath(rawPtr, buffer, buffer.Length, out retrieved);
            }

            if (result == RESULT.OK)
            {
                path = Encoding.UTF8.GetString(buffer, 0, retrieved - 1);
            }

            return result;
        }
        public RESULT unload()
        {
            RESULT result = FMOD_Studio_Bank_Unload(rawPtr);

            if (result != RESULT.OK)
            {
                return result;
            }

            rawPtr = IntPtr.Zero;

            return RESULT.OK;
        }
        public RESULT loadSampleData()
        {
            return FMOD_Studio_Bank_LoadSampleData(rawPtr);
        }
        public RESULT unloadSampleData()
        {
            return FMOD_Studio_Bank_UnloadSampleData(rawPtr);
        }
        public RESULT getLoadingState(out LOADING_STATE state)
        {
            return FMOD_Studio_Bank_GetLoadingState(rawPtr, out state);
        }
        public RESULT getSampleLoadingState(out LOADING_STATE state)
        {
            return FMOD_Studio_Bank_GetSampleLoadingState(rawPtr, out state);
        }

        // Enumeration
        public RESULT getStringCount(out int count)
        {
            return FMOD_Studio_Bank_GetStringCount(rawPtr, out count);
        }
        public RESULT getStringInfo(int index, out Guid id, out string path)
        {
            path = null;
			id = Guid.Empty;

            byte[] buffer = new byte[256];
            int retrieved = 0;
			byte[] rawguid = new byte[16];
            RESULT result = FMOD_Studio_Bank_GetStringInfo(rawPtr, index, rawguid, buffer, buffer.Length, out retrieved);

            if (result == RESULT.ERR_TRUNCATED)
            {
                buffer = new byte[retrieved];
                result = FMOD_Studio_Bank_GetStringInfo(rawPtr, index, rawguid, buffer, buffer.Length, out retrieved);
            }

            if (result == RESULT.OK)
            {
                path = Encoding.UTF8.GetString(buffer, 0, retrieved - 1);
				id = new Guid(rawguid);
            }

            return RESULT.OK;
        }

        public RESULT getEventCount(out int count)
        {
            return FMOD_Studio_Bank_GetEventCount(rawPtr, out count);
        }
        public RESULT getEventList(out EventDescription[] array)
        {
            array = null;

            RESULT result;
            int capacity;
            result = FMOD_Studio_Bank_GetEventCount(rawPtr, out capacity);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (capacity == 0)
            {
                array = new EventDescription[0];
                return result;
            }

            IntPtr[] rawArray = new IntPtr[capacity];
            int actualCount;
            result = FMOD_Studio_Bank_GetEventList(rawPtr, rawArray, capacity, out actualCount);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (actualCount > capacity) // More items added since we queried just now?
            {
                actualCount = capacity;
            }
            array = new EventDescription[actualCount];
            for (int i=0; i<actualCount; ++i)
            {
                array[i] = new EventDescription(rawArray[i]);
            }
            return RESULT.OK;
        }
        public RESULT getBusCount(out int count)
        {
            return FMOD_Studio_Bank_GetBusCount(rawPtr, out count);
        }
        public RESULT getBusList(out Bus[] array)
        {
            array = null;

            RESULT result;
            int capacity;
            result = FMOD_Studio_Bank_GetBusCount(rawPtr, out capacity);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (capacity == 0)
            {
                array = new Bus[0];
                return result;
            }

            IntPtr[] rawArray = new IntPtr[capacity];
            int actualCount;
            result = FMOD_Studio_Bank_GetBusList(rawPtr, rawArray, capacity, out actualCount);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (actualCount > capacity) // More items added since we queried just now?
            {
                actualCount = capacity;
            }
            array = new Bus[actualCount];
            for (int i=0; i<actualCount; ++i)
            {
                array[i] = new Bus(rawArray[i]);
            }
            return RESULT.OK;
        }
        public RESULT getVCACount(out int count)
        {
            return FMOD_Studio_Bank_GetVCACount(rawPtr, out count);
        }
        public RESULT getVCAList(out VCA[] array)
        {
            array = null;

            RESULT result;
            int capacity;
            result = FMOD_Studio_Bank_GetVCACount(rawPtr, out capacity);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (capacity == 0)
            {
                array = new VCA[0];
                return result;
            }

            IntPtr[] rawArray = new IntPtr[capacity];
            int actualCount;
            result = FMOD_Studio_Bank_GetVCAList(rawPtr, rawArray, capacity, out actualCount);
            if (result != RESULT.OK)
            {
                return result;
            }
            if (actualCount > capacity) // More items added since we queried just now?
            {
                actualCount = capacity;
            }
            array = new VCA[actualCount];
            for (int i = 0; i < actualCount; ++i)
            {
                array[i] = new VCA(rawArray[i]);
            }
            return RESULT.OK;
        }

        public RESULT getUserData(out IntPtr userData)
        {
            return FMOD_Studio_Bank_GetUserData(rawPtr, out userData);
        }

        public RESULT setUserData(IntPtr userData)
        {
            return FMOD_Studio_Bank_SetUserData(rawPtr, userData);
        }

        #region importfunctions
        [DllImport(STUDIO_VERSION.dll)]
        private static extern bool FMOD_Studio_Bank_IsValid(IntPtr bank);
        [DllImport(STUDIO_VERSION.dll)]
		private static extern RESULT FMOD_Studio_Bank_GetID(IntPtr bank, [Out] byte[] id);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_GetPath(IntPtr bank, [Out] byte[] path, int size, out int retrieved);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_Unload(IntPtr bank);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_LoadSampleData(IntPtr bank);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_UnloadSampleData(IntPtr bank);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_GetLoadingState(IntPtr bank, out LOADING_STATE state);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_GetSampleLoadingState(IntPtr bank, out LOADING_STATE state);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_GetStringCount(IntPtr bank, out int count);
        [DllImport(STUDIO_VERSION.dll)]
		private static extern RESULT FMOD_Studio_Bank_GetStringInfo(IntPtr bank, int index, [Out] byte[] id, [Out] byte[] path, int size, out int retrieved);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_GetEventCount(IntPtr bank, out int count);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_GetEventList(IntPtr bank, IntPtr[] array, int capacity, out int count);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_GetBusCount(IntPtr bank, out int count);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_GetBusList(IntPtr bank, IntPtr[] array, int capacity, out int count);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_GetVCACount(IntPtr bank, out int count);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_GetVCAList(IntPtr bank, IntPtr[] array, int capacity, out int count);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_GetUserData(IntPtr studiosystem, out IntPtr userData);
        [DllImport (STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_Bank_SetUserData(IntPtr studiosystem, IntPtr userData);
        #endregion

        #region wrapperinternal

        public Bank(IntPtr raw)
        : base(raw)
        {
        }

        protected override bool isValidInternal()
        {
            return FMOD_Studio_Bank_IsValid(rawPtr);
        }

        #endregion
    }

    public class CommandReplay : HandleBase
    {
        // Information query
        public RESULT getSystem(out System system)
        {
            system = null;
            IntPtr newPtr = new IntPtr();
            RESULT result = FMOD_Studio_CommandReplay_GetSystem(rawPtr, out newPtr);
            if (result == RESULT.OK)
            {
                system = new System(newPtr);
            }
            return result;
        }

        public RESULT getLength(out float totalTime)
        {
            return FMOD_Studio_CommandReplay_GetLength(rawPtr, out totalTime);
        }
        public RESULT getCommandCount(out int count)
        {
            return FMOD_Studio_CommandReplay_GetCommandCount(rawPtr, out count);
        }
        public RESULT getCommandInfo(int commandIndex, out COMMAND_INFO info)
        {
            COMMAND_INFO_INTERNAL internalInfo = new COMMAND_INFO_INTERNAL();
            FMOD.RESULT result = FMOD_Studio_CommandReplay_GetCommandInfo(rawPtr, commandIndex, out internalInfo);
            if (result != FMOD.RESULT.OK)
            {
                info = new COMMAND_INFO();
                return result;
            }
            info = internalInfo.createPublic();
            return result;
        }

        public RESULT getCommandString(int commandIndex, out string description)
        {
            description = null;
            byte[] buffer = new byte[8];
            while (true)
            {
                RESULT result = FMOD_Studio_CommandReplay_GetCommandString(rawPtr, commandIndex, buffer, buffer.Length);
                if (result == RESULT.ERR_TRUNCATED)
                {
                    buffer = new byte[2 * buffer.Length];
                }
                else 
                {
                    if (result == RESULT.OK)
                    {
                        int len = 0;
                        while (buffer[len] != 0)
                        {
                            ++len;
                        }

                        description = Encoding.UTF8.GetString(buffer, 0, len);
                    }
                    return result;
                }
            }
        }
        public RESULT getCommandAtTime(float time, out int commandIndex)
        {
            return FMOD_Studio_CommandReplay_GetCommandAtTime(rawPtr, time, out commandIndex);
        }
        // Playback
        public RESULT setBankPath(string bankPath)
        {
            return FMOD_Studio_CommandReplay_SetBankPath(rawPtr, Encoding.UTF8.GetBytes(bankPath + Char.MinValue));
        }
        public RESULT start()
        {
            return FMOD_Studio_CommandReplay_Start(rawPtr);
        }
        public RESULT stop()
        {
            return FMOD_Studio_CommandReplay_Stop(rawPtr);
        }
        public RESULT seekToTime(float time)
        {
            return FMOD_Studio_CommandReplay_SeekToTime(rawPtr, time);
        }
        public RESULT seekToCommand(int commandIndex)
        {
            return FMOD_Studio_CommandReplay_SeekToCommand(rawPtr, commandIndex);
        }
        public RESULT getPaused(out bool paused)
        {
            return FMOD_Studio_CommandReplay_GetPaused(rawPtr, out paused);
        }
        public RESULT setPaused(bool paused)
        {
            return FMOD_Studio_CommandReplay_SetPaused(rawPtr, paused);
        }
        public RESULT getPlaybackState(out PLAYBACK_STATE state)
        {
            return FMOD_Studio_CommandReplay_GetPlaybackState(rawPtr, out state);
        }
        public RESULT getCurrentCommand(out int commandIndex, out float currentTime)
        {
            return FMOD_Studio_CommandReplay_GetCurrentCommand(rawPtr, out commandIndex, out currentTime);
        }
        // Release
        public RESULT release()
        {
            return FMOD_Studio_CommandReplay_Release(rawPtr);
        }
        // Callbacks
        public RESULT setFrameCallback(COMMANDREPLAY_FRAME_CALLBACK callback)
        {
            return FMOD_Studio_CommandReplay_SetFrameCallback(rawPtr, callback);
        }
        public RESULT setLoadBankCallback(COMMANDREPLAY_LOAD_BANK_CALLBACK callback)
        {
            return FMOD_Studio_CommandReplay_SetLoadBankCallback(rawPtr, callback);
        }
        public RESULT setCreateInstanceCallback(COMMANDREPLAY_CREATE_INSTANCE_CALLBACK callback)
        {
            return FMOD_Studio_CommandReplay_SetCreateInstanceCallback(rawPtr, callback);
        }
        public RESULT getUserData(out IntPtr userData)
        {
            return FMOD_Studio_CommandReplay_GetUserData(rawPtr, out userData);
        }
        public RESULT setUserData(IntPtr userData)
        {
            return FMOD_Studio_CommandReplay_SetUserData(rawPtr, userData);
        }

        #region importfunctions
        [DllImport(STUDIO_VERSION.dll)]
        private static extern bool FMOD_Studio_CommandReplay_IsValid(IntPtr replay);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_GetSystem(IntPtr replay, out IntPtr system);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_GetLength(IntPtr replay, out float totalTime);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_GetCommandCount(IntPtr replay, out int count);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_GetCommandInfo(IntPtr replay, int commandIndex, out COMMAND_INFO_INTERNAL info);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_GetCommandString(IntPtr replay, int commandIndex, [Out] byte[] description, int capacity);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_GetCommandAtTime(IntPtr replay, float time, out int commandIndex);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_SetBankPath(IntPtr replay, byte[] bankPath);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_Start(IntPtr replay);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_Stop(IntPtr replay);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_SeekToTime(IntPtr replay, float time);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_SeekToCommand(IntPtr replay, int commandIndex);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_GetPaused(IntPtr replay, out bool paused);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_SetPaused(IntPtr replay, bool paused);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_GetPlaybackState(IntPtr replay, out PLAYBACK_STATE state);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_GetCurrentCommand(IntPtr replay, out int commandIndex, out float currentTime);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_Release(IntPtr replay);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_SetFrameCallback(IntPtr replay, COMMANDREPLAY_FRAME_CALLBACK callback);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_SetLoadBankCallback(IntPtr replay, COMMANDREPLAY_LOAD_BANK_CALLBACK callback);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_SetCreateInstanceCallback(IntPtr replay, COMMANDREPLAY_CREATE_INSTANCE_CALLBACK callback);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_GetUserData(IntPtr replay, out IntPtr userdata);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_CommandReplay_SetUserData(IntPtr replay, IntPtr userdata);
        #endregion

        #region wrapperinternal

        public CommandReplay(IntPtr raw)
        : base(raw)
        {
        }

        protected override bool isValidInternal()
        {
            return FMOD_Studio_CommandReplay_IsValid(rawPtr);
        }

        #endregion
    }

    #region wrapperinternal

    // Helper functions
    class MarshallingHelper
    {
        public static int stringLengthUtf8(IntPtr nativeUtf8)
        {
            int len = 0;
            while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;

            return len;
        }

        public static string stringFromNativeUtf8(IntPtr nativeUtf8)
        {
            // There is no one line marshal IntPtr->string for UTF8
            int len = stringLengthUtf8(nativeUtf8);
            if (len == 0) return string.Empty;
            byte[] buffer = new byte[len];
            Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, len);
        }
    }

    #endregion
} // FMOD

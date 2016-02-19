using Ads.Client.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ads.Client.Wrap
{
	public class AdsClient : Winsock.AdsClient
	{
        /// <summary>
        /// An internal list of handles and its associated lock object.
        /// </summary>
        private Dictionary<string, uint> activeSymhandles = new Dictionary<string, uint>();
        private object activeSymhandlesLock = new object();

        internal List<AdsNotification> NotificationRequests = new List<AdsNotification>();

        dynamic client;
        dynamic adsStream;
        dynamic stateInfo;
        dynamic deviceInfo;
        dynamic adsTransMode;
        object AdsTransMode_Cyclic = null;
        object AdsTransMode_OnChange = null;

        Type
            t_AdsTransMode,
            t_StateInfo,
            t_TcAdsClient,
            t_DeviceInfo,
            t_AdsVersion,
            t_AdsStream;

        /// <summary>
        /// AdsClient Constructor
        /// </summary>
        /// <param name="ipTarget">IP of the target Ads device</param>
        /// <param name="amsNetIdSource">
        /// The AmsNetId of this device. You can choose something in the form of x.x.x.x.x.x 
        /// You have to define this ID in combination with the IP as a route on the target Ads device
        /// </param>
        /// <param name="amsNetIdTarget">The AmsNetId of the target Ads device</param>
        /// <param name="amsPortTarget">Ams port. Default 801</param>
        public AdsClient(string adsApiPath, string amsNetIdSource, string ipTarget, string amsNetIdTarget, ushort amsPortTarget = 801) 
			: base(amsNetIdSource,
                ipTarget,
				amsNetIdTarget,
				amsPortTarget)
        {
            Assembly adsApi = Assembly.LoadFile(adsApiPath);
            Initialize(adsApi, amsNetIdTarget, amsPortTarget);
        }

        public AdsClient(Assembly adsApi, string amsNetIdSource, string ipTarget, string amsNetIdTarget, ushort amsPortTarget = 801)
            : base(amsNetIdSource,
                ipTarget,
                amsNetIdTarget,
                amsPortTarget)
        {
            Initialize(adsApi, amsNetIdTarget, amsPortTarget);
        }

        private void Initialize(Assembly assembly, string amsNetIdTarget, ushort amsPortTarget)
        {
            t_TcAdsClient = assembly.GetType("TwinCAT.Ads.TcAdsClient");
            t_AdsStream = assembly.GetType("TwinCAT.Ads.AdsStream");
            t_StateInfo = assembly.GetType("TwinCAT.Ads.StateInfo");
            t_DeviceInfo = assembly.GetType("TwinCAT.Ads.DeviceInfo");
            t_AdsVersion = assembly.GetType("TwinCAT.Ads.AdsVersion");
            t_AdsTransMode = assembly.GetType("TwinCAT.Ads.AdsTransMode");

            adsTransMode = Activator.CreateInstance(t_AdsTransMode);

            Array n = t_AdsTransMode.GetEnumNames();
            Array v = t_AdsTransMode.GetEnumValues();
            int c = n.Length;
            for (int i = 0; i < c; i++)
            {
                if ((string)n.GetValue(i) == "Cyclic")
                    AdsTransMode_Cyclic = v.GetValue(i);
                else
                    if ((string)n.GetValue(i) == "OnChange")
                        AdsTransMode_OnChange = v.GetValue(i);
            }

            client = Activator.CreateInstance(t_TcAdsClient);
            client.Connect(amsNetIdTarget, amsPortTarget);

            var this_eventHandler =
                typeof(AdsClient).GetMethod(
                    "TcAdsClient_AdsNotification",
                    BindingFlags.Instance | BindingFlags.NonPublic);

            client.AdsNotification +=
                (dynamic)Delegate.CreateDelegate(
                        t_TcAdsClient.GetEvent("AdsNotification").EventHandlerType,
                        this,
                        this_eventHandler);
        }

        #region Async Methods
        public override async Task<uint> GetSymhandleByNameAsync(string varName)
        {
            return await Task.FromResult(GetSymhandleByName(varName));
        }

        public override async Task<byte[]> ReadBytesAsync(uint varHandle, uint readLength)
        {
            return await Task.FromResult(ReadBytes(varHandle, readLength));
        }

        public override async Task<byte[]> ReadBytesI_Async(uint offset, uint readLength)
        {
            return await Task.FromResult(ReadBytesI(offset, readLength));
        }

        public override async Task<byte[]> ReadBytesQ_Async(uint offset, uint readLength)
        {
            return await Task.FromResult(ReadBytesQ(offset, readLength));
        }

        public override async Task ReleaseSymhandleAsync(uint symhandle)
        {
            await Task.Run(() => { ReleaseSymhandle(symhandle); }) ;
        }

        public override async Task<uint> AddNotificationAsync(uint varHandle, uint length, AdsTransmissionMode transmissionMode, uint cycleTime, object userData, Type typeOfValue)
        {
            return await Task.FromResult( AddNotification(varHandle, length, transmissionMode, cycleTime, userData, typeOfValue) );
        }

        public override async Task DeleteNotificationAsync(uint notificationHandle)
        {
            await Task.Run(() => { DeleteNotification(notificationHandle); });
        }

        public override async Task WriteBytesAsync(uint varHandle, IEnumerable<byte> varValue)
        {
            await Task.Run(() => { WriteBytes(varHandle, varValue); });
        }

        public override async Task<AdsDeviceInfo> ReadDeviceInfoAsync()
        {
            return await Task.FromResult(ReadDeviceInfo());
        }

        public override async Task<AdsState> ReadStateAsync()
        {
            return await Task.FromResult(ReadState());
        }

        public override async Task DeleteActiveNotificationsAsync()
        {
            while (NotificationRequests.Count > 0)
            {
                await DeleteNotificationAsync(NotificationRequests[0].NotificationHandle);
            }
        }

        public override async Task ReleaseActiveSymhandlesAsync()
        {
            List<uint> handles;

            lock (activeSymhandlesLock)
            {
                handles = activeSymhandles.Values.ToList();
                activeSymhandles.Clear();
            }

            foreach (var handle in handles)
                await ReleaseSymhandleAsync(handle);
        }
        #endregion

        #region Synchronous (non blocking) methods
        /// <summary>
        /// This event is called when a subscribed notification is raised
        /// </summary>
        public new event AdsNotificationDelegate OnNotification;

        private void TcAdsClient_AdsNotification(object sender, dynamic ee)
        {
            var notificationRequest = NotificationRequests.FirstOrDefault(n => n.NotificationHandle == ee.NotificationHandle);

            if (notificationRequest != null)
            {
                ee.DataStream.Position = ee.Offset;
                ee.DataStream.Read(notificationRequest.ByteValue, 0, notificationRequest.ByteValue.Length);

                if (OnNotification != null)
                {
                    OnNotification(null, new AdsNotificationArgs(notificationRequest));
                }
            }
        }

        public override byte[] ReadBytes(uint varHandle, uint readLength)
        {
            byte[] buffer = new byte[readLength];
            adsStream = Activator.CreateInstance(t_AdsStream, new object[] { buffer });

            client.Read((int)varHandle, adsStream);
            adsStream.Read(buffer, 0, (int)readLength);

            return buffer;
        }

        public override byte[] ReadBytesI(uint offset, uint readLength)
        {
            byte[] buffer = new byte[readLength];
            adsStream = Activator.CreateInstance(t_AdsStream, new object[] { buffer });

            client.Read((int)0x0000F020, (int)offset, adsStream);
            adsStream.Read(buffer, 0, (int)readLength);

            return buffer;
        }

        public override byte[] ReadBytesQ(uint offset, uint readLength)
        {
            byte[] buffer = new byte[readLength];
            adsStream = Activator.CreateInstance(t_AdsStream, new object[] { buffer });

            client.Read((int)0x0000F030, (int)offset, adsStream);
            adsStream.Read(buffer, 0, (int)readLength);

            return buffer;
        }

        public override uint GetSymhandleByName(string varName)
        {
            // Check, if the handle is already present.
            lock (activeSymhandlesLock)
            {
                if (activeSymhandles.ContainsKey(varName))
                    return activeSymhandles[varName];
            }

            // It was not retrieved before - get it from the control.
            int h = client.CreateVariableHandle(varName);

            // Now, try to add it.
            lock (activeSymhandlesLock)
            {
                if (!activeSymhandles.ContainsKey(varName))
                    activeSymhandles.Add(varName, (uint)h);

                return (uint)h;
            }
        }

        /// <summary>
        /// Release symhandle
        /// </summary>
        /// <param name="symhandle">The handle returned by GetSymhandleByName</param>
        public override void ReleaseSymhandle(uint symhandle)
        {
            // Perform a reverse-lookup at the dictionary.
            lock (activeSymhandlesLock)
            {
                foreach (var kvp in activeSymhandles)
                {
                    if (kvp.Value == symhandle)
                    {
                        activeSymhandles.Remove(kvp.Key);
                        break;
                    }
                }
            }

            client.DeleteVariableHandle( (int)symhandle );
        }

        public override void ReleaseActiveSymhandles()
        {
            List<uint> handles;

            lock (activeSymhandlesLock)
            {
                handles = activeSymhandles.Values.ToList();
                activeSymhandles.Clear();
            }

            foreach (var handle in handles)
                client.DeleteVariableHandle((int)handle);
        }

        /// <summary>
        /// Write the value to the handle returned by GetSymhandleByName
        /// </summary>
        /// <param name="varHandle">The handle returned by GetSymhandleByName</param>
        /// <param name="varValue">The byte[] value to be sent</param>
        public override void WriteBytes(uint varHandle, IEnumerable<byte> varValue)
        {
            byte[] buffer = Enumerable.ToArray(varValue);
            adsStream = Activator.CreateInstance(t_AdsStream, new object[] { buffer });
            client.Write((int)varHandle, adsStream);
        }

        /// <summary>
        /// Read the ads state 
        /// </summary>
        /// <returns></returns>
        public new AdsState ReadState()
        {
            AdsState result = new AdsState();

            stateInfo = client.ReadState();

            result.State = (ushort)((short)stateInfo.AdsState);
            result.DeviceState = (ushort)((short)stateInfo.DeviceState);

            return result;
        }

        /// <summary>
        /// Get some information of the ADS device (version, name)
        /// </summary>
        /// <returns></returns>
        public new AdsDeviceInfo ReadDeviceInfo()
        {
            AdsDeviceInfo result = new AdsDeviceInfo();

            deviceInfo = client.ReadDeviceInfo();

            result.DeviceName = deviceInfo.Name;
            result.MajorVersion = deviceInfo.Version.Version;
            result.MinorVersion = deviceInfo.Version.Revision;
            result.VersionBuild = (ushort)deviceInfo.Version.Build;

            return result;
        }

        /// <summary>
        /// Get a variable name by handle
        /// </summary>
        /// <param name="handle">Symhandle</param>
        /// <returns>A twincat variable like ".XXX" or null if didn't find</returns>
        public virtual string GetNameBySymhandle(uint symhandle)
        {
            string name = null;

            lock (activeSymhandlesLock)
            {
                foreach (var kvp in activeSymhandles)
                {
                    if (kvp.Value == symhandle)
                    {
                        name = kvp.Key;
                        break;
                    }
                }
            }

            return name;
        }

        /// <summary>
        /// Add a noticiation when a variable changes or cyclic after a defined time in ms
        /// </summary>
        /// <param name="varHandle">The handle returned by GetSymhandleByName</param>
        /// <param name="length">The length of the data that must be send by the notification</param>
        /// <param name="transmissionMode">On change or cyclic</param>
        /// <param name="cycleTime">The cyclic time in ms. If used with OnChange, then the value is send once after this time in ms</param>
        /// <param name="userData">A custom object that can be used in the callback</param>
        /// <param name="TypeOfValue">The type of the returned notification value</param>
        /// <returns>The notification handle</returns>
        public override uint AddNotification(uint varHandle, uint length, AdsTransmissionMode transmissionMode, uint cycleTime, object userData, Type TypeOfValue)
        {
            AdsNotification note = new AdsNotification();
            note.Symhandle = varHandle;
            note.UserData = userData;
            note.TypeOfValue = TypeOfValue;
            note.ByteValue = new byte[length];
            NotificationRequests.Add(note);

            string varName = GetNameBySymhandle(varHandle);
            byte[] buffer = new byte[length];
            adsStream = Activator.CreateInstance(t_AdsStream, new object[] { buffer });

            if (transmissionMode == AdsTransmissionMode.Cyclic)
                adsTransMode = AdsTransMode_Cyclic;
            else
                adsTransMode = AdsTransMode_OnChange;

            note.NotificationHandle = 
                (uint)client.AddDeviceNotification(
                    varName,
                    adsStream, (int)0, (int)length,
                    adsTransMode, (int)cycleTime, (int)0,
                    userData);

            return note.NotificationHandle;
        }

        /// <summary>
        /// Delete a previously registerd notification
        /// </summary>
        /// <param name="notificationHandle">The handle returned by AddNotification</param>
        /// <returns></returns>
        public override void DeleteNotification(uint notificationHandle)
        {
            client.DeleteDeviceNotification((int)notificationHandle);

            var notification = NotificationRequests.FirstOrDefault(n => n.NotificationHandle == notificationHandle);

            if (notification != null)
                NotificationRequests.Remove(notification);
        }

        public override void DeleteNotification(string varName)
        {
            uint varHandle = GetSymhandleByName(varName);
            var notificationHandle = NotificationRequests.First(request => request.Symhandle == varHandle).NotificationHandle;
            DeleteNotification(notificationHandle);
        }

        public override void DeleteActiveNotifications()
        {
            if (NotificationRequests != null)
            {
                while (NotificationRequests.Count > 0)
                {
                    DeleteNotification(NotificationRequests[0].NotificationHandle);
                }
            }
        }

        #endregion
    }
}


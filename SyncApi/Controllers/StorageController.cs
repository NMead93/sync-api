using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SyncApi.Models;
using SyncApi.Helpers;
using SyncApi.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using SyncApi.SyncEnums;

namespace SyncApi.Controllers
{
    public class StorageController : ApiController
    {
        

        //api/[controller]/GetAllPreferences
        public IEnumerable<UserPreferenceEntity> GetAllPreferences()
        {
            UserPreferenceEntity preferenceManager = new UserPreferenceEntity();
            IEnumerable<UserPreferenceEntity> preferenceRecords = (IEnumerable<UserPreferenceEntity>)preferenceManager.GetAll();

            return preferenceRecords;
        }

        // api/[controller]/AddPreference
        [HttpPost]
        public IHttpActionResult AddPreference([FromBody]UserPreferenceEntity newPreference)
        {
            ServerCUDDataStore dataStore = new ServerCUDDataStore();
            try
            {
                string payload = JsonConvert.SerializeObject(newPreference);
                newPreference.Create();
                dataStore.AddCUDEntity(newPreference.TableName, RecordIdHelper.CreateRecordId(newPreference), newPreference.CustomerId, payload, "Create", DateTimeOffset.Now);

                return Ok("Success!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // api/[controller]/DeletePreference
        [HttpPost]
        public IHttpActionResult DeletePreference([FromBody]UserPreferenceEntity preferenceToDelete)
        {
            ServerCUDDataStore dataStore = new ServerCUDDataStore();
            try
            {
                //call azure service delete
                string payload = JsonConvert.SerializeObject(preferenceToDelete);
                preferenceToDelete.Delete();

                string recordIdCUD = preferenceToDelete.CustomerId + "_" + preferenceToDelete.PreferenceName;
                dataStore.AddCUDEntity(preferenceToDelete.TableName, recordIdCUD, preferenceToDelete.CustomerId, JsonConvert.SerializeObject(preferenceToDelete), "Delete", DateTimeOffset.Now);

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // api/[controller]/UpdatePreference
        [HttpPost]
        public IHttpActionResult UpdatePreference([FromBody]UserPreferenceEntity updatedPreference)
        {
            ServerCUDDataStore dataStore = new ServerCUDDataStore();
            try
            {
                updatedPreference.Update();
                string recordIdCUD = updatedPreference.CustomerId + "_" + updatedPreference.PreferenceName;
                dataStore.AddCUDEntity(updatedPreference.TableName, recordIdCUD, updatedPreference.CustomerId, JsonConvert.SerializeObject(updatedPreference), "Update", DateTimeOffset.Now);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //api/[controller]/SyncMobile
        [HttpPost]
        public async Task<IHttpActionResult> SyncMobile([FromBody]SyncInfo clientSyncInfo)
        {
            ServerCUDDataStore dataStore = new ServerCUDDataStore();
            DataSyncManager manager = new DataSyncManager(dataStore, new ServerCustomDeserialize(), Enums.DataLocation.Server, DateTimeOffset.Now);
            await manager.Sync(clientSyncInfo.ClientOperations);
            IEnumerable<CUDEntity> clientDirections = await dataStore.CUDSinceSync(clientSyncInfo.LastSync);
            //var CUDPayload = await Task.Run(() => JsonConvert.SerializeObject(clientDirections));
            //var httpContent = new StringContent(CUDPayload, Encoding.UTF8, "application/json");

            return Ok(clientDirections);
        }
    }
}
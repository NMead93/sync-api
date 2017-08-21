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
using SyncApi.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

namespace SyncApi.Controllers
{
    public class StorageController : ApiController
    {
        

        //api/[controller]/Get
        public IEnumerable<UserPreferenceEntity> Get()
        {
            IEnumerable<UserPreferenceEntity> preferenceRecords = AzureTableService.GetPreferences();

            return preferenceRecords;
        }

        // api/[controller]/AddPreference
        [HttpPost]
        public IHttpActionResult AddPreference([FromBody]UserPreferenceEntity newPreference)
        {
            try
            {
                string payload = JsonConvert.SerializeObject(newPreference);
                AzureTableService.AddPreference(newPreference);
                AzureTableService.InsertCUDEntity(newPreference.TableName, RecordIdHelper.CreateRecordId(payload), newPreference.CustomerId, payload, "Create", DateTimeOffset.Now);

                return Ok("Success!");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // api/[controller]/DeletePreference
        [HttpPost]
        public IHttpActionResult DeletePreference([FromBody]UserPreferenceEntity preferenceToDelete)
        {
            try
            {
                //call azure service delete
                string payload = JsonConvert.SerializeObject(preferenceToDelete);
                AzureTableService.DeletePreference(preferenceToDelete);

                string recordIdCUD = preferenceToDelete.CustomerId + "_" + preferenceToDelete.PreferenceName;
                AzureTableService.InsertCUDEntity(preferenceToDelete.TableName, recordIdCUD, preferenceToDelete.CustomerId, JsonConvert.SerializeObject(preferenceToDelete), "Delete", DateTimeOffset.Now);

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
            try
            {
                AzureTableService.UpdatePreference(updatedPreference);
                string recordIdCUD = updatedPreference.CustomerId + "_" + updatedPreference.PreferenceName;
                AzureTableService.InsertCUDEntity(updatedPreference.TableName, recordIdCUD, updatedPreference.CustomerId, JsonConvert.SerializeObject(updatedPreference), "Update", DateTimeOffset.Now);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //api/[controller]/SyncMobile
        [HttpPost]
        public IHttpActionResult SyncMobile([FromBody]SyncInfo clientSyncInfo)
        {
            IEnumerable<CUDEntity> clientDirections =  AzureTableService.UpdateServerStorage(clientSyncInfo.ClientOperations, clientSyncInfo.LastSync);
            //var CUDPayload = await Task.Run(() => JsonConvert.SerializeObject(clientDirections));
            //var httpContent = new StringContent(CUDPayload, Encoding.UTF8, "application/json");

            return Ok(clientDirections);
        }
    }
}
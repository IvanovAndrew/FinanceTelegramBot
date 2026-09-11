namespace Infrastructure.Test;

public class FnsApiTest
{
//     [Fact]
//     public async Task T()
//     {
//         const string response = """
// {
//     "code": 3,
//     "user": "ООО \"Агроторг\"",
//     "items": [
//         {
//             "nds": 11,
//             "sum": 11999,
//             "name": "КРУП.Шоколад ОСОБ.тем.ап/гр.85г",
//             "price": 11999,
//             "quantity": 1,
//             "paymentType": 4,
//             "productType": 1,
//             "itemsQuantityMeasure": 0
//         },
//         {
//             "nds": 11,
//             "sum": 7699,
//             "name": "ПСЫЖ Вода мин.пит.леч/ст.газ.1л",
//             "price": 7699,
//             "quantity": 1,
//             "paymentType": 4,
//             "productType": 33,
//             "productCodeNew": {
//                 "gs1m": {
//                     "gtin": "04605035006964",
//                     "sernum": "5OoyE8qxeIOn>",
//                     "productIdType": 6,
//                     "rawProductCode": "0104605035006964215OoyE8qxeIOn>"
//                 }
//             },
//             "labelCodeProcesMode": 0,
//             "itemsIndustryDetails": [
//                 {
//                     "idFoiv": "030",
//                     "industryPropValue": "UUID=42a6a883-d6ea-4279-8c1b-08ecb8d75169&Time=1785764594397",
//                     "foundationDocNumber": "1944",
//                     "foundationDocDateTime": "21.11.2023"
//                 }
//             ],
//             "itemsQuantityMeasure": 0,
//             "checkingProdInformationResult": 15
//         },
//         {
//             "nds": 2,
//             "sum": 5999,
//             "name": "ПИСК.Биоpяженка 4% 500г",
//             "price": 5999,
//             "quantity": 1,
//             "paymentType": 4,
//             "productType": 33,
//             "productCodeNew": {
//                 "gs1m": {
//                     "gtin": "04602547000046",
//                     "sernum": "5IADe0",
//                     "productIdType": 6,
//                     "rawProductCode": "0104602547000046215IADe0"
//                 }
//             },
//             "labelCodeProcesMode": 0,
//             "itemsIndustryDetails": [
//                 {
//                     "idFoiv": "030",
//                     "industryPropValue": "UUID=766d91af-f418-48db-b2f3-2c7d4bec423c&Time=1785764607912",
//                     "foundationDocNumber": "1944",
//                     "foundationDocDateTime": "21.11.2023"
//                 }
//             ],
//             "itemsQuantityMeasure": 0,
//             "checkingProdInformationResult": 15
//         },
//         {
//             "nds": 2,
//             "sum": 4199,
//             "name": "Д.В Д.Творог МЯГКИЙ 5% 150г",
//             "price": 4199,
//             "quantity": 1,
//             "paymentType": 4,
//             "productType": 33,
//             "productCodeNew": {
//                 "gs1m": {
//                     "gtin": "04690228112072",
//                     "sernum": "5LKh'o",
//                     "productIdType": 6,
//                     "rawProductCode": "0104690228112072215LKh'o"
//                 }
//             },
//             "labelCodeProcesMode": 0,
//             "itemsIndustryDetails": [
//                 {
//                     "idFoiv": "030",
//                     "industryPropValue": "UUID=985d33f3-4f66-4e8d-911c-2c7c4dd3bc2d&Time=1785764604374",
//                     "foundationDocNumber": "1944",
//                     "foundationDocDateTime": "21.11.2023"
//                 }
//             ],
//             "itemsQuantityMeasure": 0,
//             "checkingProdInformationResult": 15
//         },
//         {
//             "nds": 2,
//             "sum": 4199,
//             "name": "Д.В Д.Творог МЯГКИЙ 5% 150г",
//             "price": 4199,
//             "quantity": 1,
//             "paymentType": 4,
//             "productType": 33,
//             "productCodeNew": {
//                 "gs1m": {
//                     "gtin": "04690228112072",
//                     "sernum": "5UA:lm",
//                     "productIdType": 6,
//                     "rawProductCode": "0104690228112072215UA:lm"
//                 }
//             },
//             "labelCodeProcesMode": 0,
//             "itemsIndustryDetails": [
//                 {
//                     "idFoiv": "030",
//                     "industryPropValue": "UUID=b1480ef4-f555-4649-bbe1-4f738a96da1c&Time=1785764616235",
//                     "foundationDocNumber": "1944",
//                     "foundationDocDateTime": "21.11.2023"
//                 }
//             ],
//             "itemsQuantityMeasure": 0,
//             "checkingProdInformationResult": 15
//         },
//         {
//             "nds": 2,
//             "sum": 4199,
//             "name": "Д.В Д.Творог МЯГКИЙ 5% 150г",
//             "price": 4199,
//             "quantity": 1,
//             "paymentType": 4,
//             "productType": 33,
//             "productCodeNew": {
//                 "gs1m": {
//                     "gtin": "04690228112072",
//                     "sernum": "5PP&d8",
//                     "productIdType": 6,
//                     "rawProductCode": "0104690228112072215PP&d8"
//                 }
//             },
//             "labelCodeProcesMode": 0,
//             "itemsIndustryDetails": [
//                 {
//                     "idFoiv": "030",
//                     "industryPropValue": "UUID=e050dc46-c90a-4737-9d78-8c9ef28fd276&Time=1785764621340",
//                     "foundationDocNumber": "1944",
//                     "foundationDocDateTime": "21.11.2023"
//                 }
//             ],
//             "itemsQuantityMeasure": 0,
//             "checkingProdInformationResult": 15
//         },
//         {
//             "nds": 11,
//             "sum": 4999,
//             "name": "DIR.Жев.рез.X-FR.М.СВ.м.вк.16г",
//             "price": 4999,
//             "quantity": 1,
//             "paymentType": 4,
//             "productType": 1,
//             "itemsQuantityMeasure": 0
//         },
//         {
//             "nds": 2,
//             "sum": 12999,
//             "name": "БЗМЖ ПРОСТ.Мол.б/л у/п1,5%970мл",
//             "price": 12999,
//             "quantity": 1,
//             "paymentType": 4,
//             "productType": 33,
//             "productCodeNew": {
//                 "gs1m": {
//                     "gtin": "04600605026533",
//                     "sernum": "5Vv*fc",
//                     "productIdType": 6,
//                     "rawProductCode": "0104600605026533215Vv*fc"
//                 }
//             },
//             "labelCodeProcesMode": 0,
//             "itemsIndustryDetails": [
//                 {
//                     "idFoiv": "030",
//                     "industryPropValue": "UUID=44b0cc91-e2e2-45c8-aac0-b9b9678ed52a&Time=1785764630892",
//                     "foundationDocNumber": "1944",
//                     "foundationDocDateTime": "21.11.2023"
//                 }
//             ],
//             "itemsQuantityMeasure": 0,
//             "checkingProdInformationResult": 15
//         }
//     ],
//     "nds10": 2872,
//     "fnsUrl": "www.nalog.gov.ru",
//     "region": "78",
//     "userInn": "7825706086  ",
//     "dateTime": "2026-08-03T16:44:00",
//     "kktRegId": "0006828493046904    ",
//     "metadata": {
//         "id": 6472058617983623000,
//         "ofdId": "ofd22",
//         "address": "192286,Россия,г. Санкт-Петербург,муниципальный округ Георгиевский вн.тер.г.,,,,Димитрова ул,,Дом 18,Корпус 1,Литер А,Помещение 8-Н, 9-Н,",
//         "subtype": "receipt",
//         "receiveDate": "2026-08-03T16:45:51Z"
//     },
//     "totalSum": 56292,
//     "creditSum": 0,
//     "numberKkt": "0141870012176301",
//     "fiscalSign": 3576585508,
//     "prepaidSum": 0,
//     "properties": {
//         "propertyName": "X5",
//         "propertyValue": "5564;POS70-BO-5564;61d0f23f9d784298b7762665c671579e;cee5504494264d87ddd5cf7593bfc928"
//     },
//     "retailPlace": "9-Н; 5564 310-Пятерочка",
//     "shiftNumber": 245,
//     "cashTotalSum": 0,
//     "provisionSum": 0,
//     "ecashTotalSum": 56292,
//     "machineNumber": "0141870012176301",
//     "operationType": 1,
//     "redefine_mask": 0,
//     "requestNumber": 73,
//     "amountsReceiptNds": {
//         "amountsNds": [
//             {
//                 "nds": 11,
//                 "ndsSum": 4454
//             }
//         ]
//     },
//     "fiscalDriveNumber": "7384440900844678",
//     "messageFiscalSign": 9297263423549960000,
//     "retailPlaceAddress": "192286,город федерального значения Санкт-Петербург,вн.тер.г. м. о. Георгиевский,ул Димитрова,д. 18 к. 1 литера А, помещ. 8-Н,",
//     "appliedTaxationType": 1,
//     "fiscalDocumentNumber": 43553,
//     "fiscalDocumentFormatVer": 4,
//     "checkingLabeledProdResult": 0
// }
// """;
//         
//         var options = new JsonSerializerOptions
//         {
//             PropertyNameCaseInsensitive = true
//         };
//         var fnsApiStub = new FnsApiStub
//         {
//             Response = new FnsResponse()
//             {
//                 Data = new FnsData(){Json = JsonConvert.DeserializeObject<FnsCheckInfo>(response)}
//             } 
//         };
//         
//         var loadingResult = await new FnsApiService(fnsApiStub, "token", new LoggerStub<FnsApiService>()).GetCheck(new CheckRequisite());
//         
//         Assert.True(loadingResult.Success);
//         Assert.NotEmpty(loadingResult.Data?.Where(x => x.ProductInternationalCode is not null));
//     }
}
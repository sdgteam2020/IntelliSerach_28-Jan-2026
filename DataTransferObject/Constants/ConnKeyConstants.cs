namespace DataTransferObject.Constants
{
    public static class ConnKeyConstants
    {
        #region Return To Front End

        public const int Success = 200;
        public const int BadRequest = 400;
        public const int InternalServerError = 500;
        public const int Save = 1;
        public const int Update = 2;
        public const int Exists = 3;
        public const int IncorrectData = 4;

        public const string SuccessMessage = "The operation was successful.";
        public const string NotPdfFile = "please Upload Pdf File Only.";
        public const string BadRequestMessage = "The request was invalid.";
        public const string InternalServerErrorMessage = "An unexpected error occurred. Please try again later.";
        public const string SaveMessage = "Data has been successfully saved.";
        public const string UpdateMessage = "Data has been successfully updated.";
        public const string ExistsMessage = "The resource already exists.";
        public const string IncorrectDataMessage = "The data provided is incorrect or incomplete.";
        public const string UploadSucess = "Upload successfully.";

        public const string ScraperingMessage = "Scrapering Done.";

        public const string SuccessGet = "Data successful Get.";

        #endregion Return To Front End
    }
}
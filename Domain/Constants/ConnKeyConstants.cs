namespace Domain.Constants
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
        public const string RegistrationSuccessMessage = "Registration successful!";
        public const string LoggedInMessage = "Successfully Logged In.";
        public const string InvalidCredentialsMessage = "Invalid username or password.";
        public const string RoleMismatchMessage = "You do not have permission to log in. Your role does not match.";
        public const string AccountLockedMessage = "Account Locked Out Please Try after 10 minutes.";
        public const string SignInNotAllowedFormat = "Sign-in not allowed for user {0}.";
        public const string RequestCanceledMessage = "Request canceled.";
        public const string SaltNullMessage = "Salt is null";
        public const string UserAlreadyExistsMessage = "User already exists.";
        public const string UserNotFoundMessage = "User not found.";
        public const string NotPdfFile = "please Upload Pdf File Only.";
        public const string BadRequestMessage = "The request was invalid.";
        public const string InternalServerErrorMessage = "An unexpected error occurred. Please try again later.";
        public const string SaveMessage = "Data has been successfully saved.";
        public const string UpdateMessage = "Data has been successfully updated.";
        public const string ExistsMessage = "The resource already exists.";
        public const string IncorrectDataMessage = "The data provided is incorrect or incomplete.";


        public const string UploadSucess = "Upload successfully.";
        public const string FileNotExists = "The specified file was not found.";
        public const string FileDeleteSucess = "File has been deleted successfully.";
        public const string FileDeleteFailed = "Failed to delete the file.";
        public const string InvalidFileName = "Invalid file name provided.";
        public const string NotAuth = "You are not authorized to perform this action.";





        public const string ScraperingMessage = "Scrapering Done.";

        public const string SuccessGet = "Data successful Get.";

        #endregion Return To Front End
    }
}
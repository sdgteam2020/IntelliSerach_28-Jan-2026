using Microsoft.AspNetCore.Http;

namespace BusinessLogicsLayer.Service
{
    public class ServiceRepository : IService
    {
        public string ProcessUploadedFile(IFormFile UploadDoc, string FileAddress, string FileName)
        {
            string? uniqueFileName = null;
            string ext = System.IO.Path.GetExtension(UploadDoc.FileName);
            // uniqueFileName = FileName + ext;// Guid.NewGuid().ToString() + ext;
            string filePath = Path.Combine(FileAddress, FileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                UploadDoc.CopyTo(fileStream);
            }
            return FileName;
        }

        public bool IsPdf(IFormFile postedFile)
        {
            //-------------------------------------------
            //  Check the PDF mime type
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "application/pdf")
            {
                return false;
            }

            //-------------------------------------------
            //  Check the PDF extension
            //-------------------------------------------
            if (Path.GetExtension(postedFile.FileName).ToLower() != ".pdf")
            {
                return false;
            }

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try
            {
                if (!postedFile.OpenReadStream().CanRead)
                {
                    return false;
                }

                // Minimum valid PDF size (header + body + footer)
                if (postedFile.Length < 4)
                {
                    return false;
                }

                // Read first 5 bytes (PDF header always starts with "%PDF-")
                byte[] buffer = new byte[5];
                using (var stream = postedFile.OpenReadStream())
                {
                    stream.Read(buffer, 0, 5);
                }

                string header = System.Text.Encoding.ASCII.GetString(buffer);

                if (!header.StartsWith("%PDF-"))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                // Reset stream position (important if you want to save file later)
                postedFile.OpenReadStream().Position = 0;
            }

            return true;
        }
    }
}
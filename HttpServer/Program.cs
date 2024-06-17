using System.Net;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        // Create a new HttpListener instance
        HttpListener listener = new HttpListener();

        // Define the prefixes (URL and port) the server will respond to
        listener.Prefixes.Add("http://localhost:8080/");

        // Start the HttpListener
        listener.Start();
        Console.WriteLine("Listening...");

        // Handle requests asynchronously
        Task.Run(() =>
        {
            while (true)
            {
                // Wait for an incoming request
                HttpListenerContext context = listener.GetContext();

                // Get the request and response objects
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // Get the requested file path
                string localPath = request.Url.LocalPath.TrimStart('/');
                if (string.IsNullOrEmpty(localPath))
                {
                    localPath = "index.html";
                }
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), localPath);

                // Check if the file exists
                if (File.Exists(filePath))
                {
                    try
                    {
                        // Read the file content
                        byte[] buffer = File.ReadAllBytes(filePath);

                        // Set the response content type based on the file extension
                        response.ContentType = GetContentType(filePath);

                        // Set the response content length
                        response.ContentLength64 = buffer.Length;

                        // Write the file content to the response output stream
                        using (Stream output = response.OutputStream)
                        {
                            output.Write(buffer, 0, buffer.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error serving file {filePath}: {ex.Message}");
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }
                else
                {
                    // Handle file not found
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    byte[] buffer = Encoding.UTF8.GetBytes("<html><body>404 - File Not Found</body></html>");
                    response.ContentLength64 = buffer.Length;
                    using (Stream output = response.OutputStream)
                    {
                        output.Write(buffer, 0, buffer.Length);
                    }
                }

                response.Close();
            }
        });

        // Prevent the application from closing immediately
        Console.WriteLine("Press any key to stop the server...");
        Console.ReadKey();

        // Stop the HttpListener
        listener.Stop();
    }

    static string GetContentType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        switch (extension)
        {
            case ".htm":
            case ".html":
                return "text/html";
            case ".css":
                return "text/css";
            case ".js":
                return "application/javascript";
            case ".png":
                return "image/png";
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".gif":
                return "image/gif";
            default:
                return "application/octet-stream";
        }
    }
}

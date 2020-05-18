
public static class Constants
{
    public static string PREFIX = "[GreeterMA] ";
    public static string STANDBY = PREFIX + "Press red button to capture photo";
    public static string NONE = "none";

    // Success
    public static string API_KEY_LOADED = PREFIX + "Api key loaded";

    // Error texts
    public static string API_KEY_NOT_LOADED = PREFIX + "API Key not loaded";
    public static string CANVAS_NOT_FOUND = PREFIX + "Main canvas not found";
    public static string UI_MANAGER_NOT_FOUND = PREFIX + "UI Manager not found";
    public static string AZURE_PERSON_GROUP_CREATION_FAILED = PREFIX + "Azure PersonGroup creation failed";
    public static string AZURE_PERSONGROUPLIST_EMPTY_OR_ERROR = PREFIX + "Azure PersonGroup either empty or an error occurred";

    // Error codes
    public static int AZURE_PERSON_GROUP_CREATION_FAILED_CODE = 10001;
    public static int AZURE_PERSONGROUPLIST_EMPTY_OR_ERROR_CODE = 10002;

    // In process
    public static string CAPTURING = PREFIX + "Capturing";
}

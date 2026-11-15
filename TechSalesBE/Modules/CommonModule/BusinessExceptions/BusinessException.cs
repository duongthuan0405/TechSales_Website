namespace CommonModule.BusinessExceptions;
public class BusinessException : Exception
{
    private Dictionary<string, List<string>> _errors = new();
    public string Code { get; }

    public Dictionary<string, List<string>> Errors
    {
        get =>
            _errors.ToDictionary(
                error => error.Key, 
                error => error.Value.ToList()
            );
    }

    public void AddError(string key, string error)
    {
        bool isExist = _errors.TryGetValue(key, out List<string>? errorLists);
        if(isExist)
        {
            errorLists?.Add(error);
        }
        else
        {
            _errors.Add(key, new List<string>() {error});
        }
    }

    public BusinessException(string code, string message = "", Dictionary<string, List<string>>? errors = null) 
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        Code = code;

        if(errors != null)
        {
            _errors = errors;
        }
    }

}

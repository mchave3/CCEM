namespace CCEM.Core.Logger;

/// <summary>
/// Application-facing logging abstraction that hides Serilog specifics.
/// </summary>
public interface IAppLogger
{
    void Verbose(string messageTemplate, params object[] propertyValues);
    void Debug(string messageTemplate, params object[] propertyValues);
    void Information(string messageTemplate, params object[] propertyValues);
    void Warning(string messageTemplate, params object[] propertyValues);
    void Error(string messageTemplate, params object[] propertyValues);
    void Error(Exception exception, string messageTemplate, params object[] propertyValues);
    void Fatal(string messageTemplate, params object[] propertyValues);
    void Fatal(Exception exception, string messageTemplate, params object[] propertyValues);
}

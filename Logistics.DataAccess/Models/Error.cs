namespace Logistics.DataAccess.Models;

public struct Error
{
  public ErrorCode Code { get; }
  public string Message { get; }
  public DateTime Date { get; }

  public Error()
  {
    Code = ErrorCode.None;
    Message = string.Empty;
    Date = DateTime.UtcNow;
  }

  public Error(ErrorCode errorCode, string errorMessage)
  {
    Code = errorCode;
    Message = errorMessage;
    Date = DateTime.UtcNow;
  }
}

public enum ErrorCode
{
  // Success
  None,

  // Errors
  NotFound,

  ParsingError,
  ValidationError,
  MongoError,
  ServerError,
  Unknown
}

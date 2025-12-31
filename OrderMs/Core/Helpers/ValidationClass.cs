using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace Core.Helpers;

public class Errors : IEnumerable<Error>
{
    private readonly ICollection<Error> errors = [];

    public void RegisterError(Enum errorEnum, string message) => errors.Add(new Error(errorEnum, message));
    public void RegisterError(Enum errorEnum, string message, params string?[] propertyName) => errors.Add(new Error(errorEnum, message, propertyName));

    public IEnumerator<Error> GetEnumerator()
    {
        return errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return errors.GetEnumerator();
    }

    public string Summary
    {
        get
        {
            var sb = new StringBuilder();
            foreach (var item in errors)
                sb.AppendLine($"{item.ErrorEnum} - {item.Message}");
            return sb.ToString();
        }
    }

    public override string ToString()
    {
        return $"{Summary}";
    }
}

public class Error()
{

    public Enum ErrorEnum { get; set; }

    public string?[] PropertyNames { get; set; }

    public string Message { get; set; }

    public string PropertiesNamesSummary
    {
        get
        {
            StringBuilder sb = new StringBuilder();

            if (PropertyNames == null || PropertyNames.Length == 0)
                return string.Empty;

            foreach (string propertyName in PropertyNames)
            {
                sb.Append($", {propertyName}");
            }

            return sb.ToString()[2..];
        }
    }
    public Error(Enum errorEnum, string message) : this()
    {
        ErrorEnum = errorEnum;
        Message = message;
    }

    public Error(Enum errorEnum, string message, params string[] propertyName) : this()
    {
        ErrorEnum = errorEnum;
        PropertyNames = propertyName;
        Message = message;
    }
}

public enum GenericErrors
{
    NegativeIdError,
    IdZeroError,
    NegativeValueError,
    ValueZeroError,
    EmptyStringError,
    StringSizeExcedeedError,
    StringMinSizeNotReachedError,
    StringOutOfSizeRangeError,
    NullValueError,
    InvalidObjectError,
    InvalidBsonIdError,
    SameDateTimeError,
    StartBiggerThanEndDateTimeError,
    DateTimeBiggerThanNowError,
    InvalidIpError,
    InvalidTokenError
}

/// <summary>
/// Classe para validação de entidades, com diversos métodos para validação de forma mais simplificada
/// </summary>
public abstract class ValidatorClass
{
    private readonly Errors errors = [];

    [JsonIgnore]
    public bool IsValid => !errors.Any();
    [JsonIgnore, NotMapped]
    public Errors Errors => errors;

    protected abstract void Validate();

    public bool ContainsError(Enum errorEnum) => errors.Any(x => x.ErrorEnum.ToString() == errorEnum.ToString());
    public bool ContainsError(Enum errorEnum, string propertyName) => errors.Any(x => x.ErrorEnum.ToString() == errorEnum.ToString() && x.PropertyNames.Contains(propertyName));
    public bool ContainsError(int enumValue, string propertyName) => errors.Any(x => Convert.ToInt32(x.ErrorEnum) == enumValue && x.PropertyNames.Contains(propertyName));
    public bool ContainsError(Enum errorEnum, string[] propertyNames) => errors.Any(x => x.ErrorEnum.ToString() == errorEnum.ToString() && x.PropertyNames.Intersect(propertyNames).Any());
    public bool ContainsError(int enumValue, string[] propertyNames) => errors.Any(x => Convert.ToInt32(x.ErrorEnum) == enumValue && x.PropertyNames.Intersect(propertyNames).Any());

    protected void IdValidation(int id, string? propertyName = null, bool validateZero = false)
    {
        string propertyNameToConsider = propertyName != null ? propertyName : "Id";

        if (!int.IsPositive(id))
            Errors.RegisterError(GenericErrors.NegativeIdError, $"'{propertyNameToConsider}' não pode ser negativo.", propertyName);

        if (validateZero && id == 0)
            Errors.RegisterError(GenericErrors.IdZeroError, $"'{propertyNameToConsider}' não pode ser 0", propertyName);
    }

    protected void IdValidation(long id, string? propertyName = null, bool validateZero = false)
    {
        string propertyNameToConsider = propertyName != null ? propertyName : "Id";

        if (!long.IsPositive(id))
            Errors.RegisterError(GenericErrors.NegativeIdError, $"'{propertyNameToConsider}' não pode ser negativo.", propertyName);

        if (validateZero && id == 0)
            Errors.RegisterError(GenericErrors.IdZeroError, $"'{propertyNameToConsider}' não pode ser 0", propertyName);
    }

    protected void PositiveValueValidation(string propertyName, int value, bool validateZero = false)
    {
        if (!int.IsPositive(value))
            Errors.RegisterError(GenericErrors.NegativeValueError, $"'{propertyName}' não pode ser negativo(a).", propertyName);

        if (validateZero && value == 0)
            Errors.RegisterError(GenericErrors.ValueZeroError, $"'{propertyName}' não pode ser 0", propertyName);
    }

    protected void PositiveValueValidation(string propertyName, long value, bool validateZero = false)
    {
        if (!long.IsPositive(value))
            Errors.RegisterError(GenericErrors.NegativeValueError, $"'{propertyName}' não pode ser negativo(a).", propertyName);

        if (validateZero && value == 0)
            Errors.RegisterError(GenericErrors.ValueZeroError, $"'{propertyName}' não pode ser 0", propertyName);
    }

    protected void PositiveValueValidation(string propertyName, double value, bool validateZero = false)
    {
        if (!double.IsPositive(value))
            Errors.RegisterError(GenericErrors.NegativeValueError, $"'{propertyName}' não pode ser negativo(a).", propertyName);

        if (validateZero && value == 0)
            Errors.RegisterError(GenericErrors.ValueZeroError, $"'{propertyName}' não pode ser 0", propertyName);
    }

    protected void NotEmptyStringValidation(string propertyName, string? propertyValue)
    {
        if (string.IsNullOrEmpty(propertyValue))
            Errors.RegisterError(GenericErrors.EmptyStringError, $"'{propertyName}' não informado(a)!", propertyName);
    }

    protected void NotEmptyStringLengthValidation(string propertyName, string? propertyValue, int maxLength)
    {
        NotEmptyStringValidation(propertyName, propertyValue);

        if (propertyValue.Length > maxLength)
            Errors.RegisterError(GenericErrors.StringSizeExcedeedError, $"'{propertyName}' deve possuir até {maxLength} caracteres", propertyName);
    }

    protected void StringLengthValidation(string propertyName, string? propertyValue, int minLength, int maxLength)
    {
        if (minLength > maxLength)
            throw new ArgumentException("O valor mínimo deve ser menor ou igual ao valor máximo.", propertyName);

        if (propertyValue.Length != minLength && minLength == maxLength)
            Errors.RegisterError(GenericErrors.StringMinSizeNotReachedError, $"'{propertyName}' deve possuir {minLength} caracteres.", propertyName);

        if (propertyValue.Length < minLength || propertyValue.Length > maxLength)
            Errors.RegisterError(GenericErrors.StringOutOfSizeRangeError, $"'{propertyName}' deve possuir entre {minLength} e {maxLength} caracteres.", propertyName);
    }

    protected void NotNullValueValidation(object obj, string propertyName)
    {
        if (obj == null)
            Errors.RegisterError(GenericErrors.NullValueError, $"'{propertyName}' não pode ser nulo!", propertyName);
    }

    protected void ValidObjectValidation(ValidatorClass objWithValidator, string propertyName)
    {
        if (!objWithValidator.IsValid)
        {
            Errors.RegisterError(GenericErrors.InvalidObjectError, $"'{propertyName}' possui {objWithValidator.Errors.Count()} erro(s)");

            foreach (var erro in objWithValidator.Errors)
            {
                Errors.RegisterError(erro.ErrorEnum, $"'{propertyName}' -> " + erro.Message, erro.PropertiesNamesSummary);
            }
        }
    }

    protected void StartEndDateTimeValidation(DateTime startDateTime, string startDateTimePropertyName, DateTime endDateTime, string endDateTimePropertyName, bool validateSameDateTimes = false)
    {
        if (validateSameDateTimes && startDateTime == endDateTime)
            Errors.RegisterError(GenericErrors.SameDateTimeError, $"'{startDateTimePropertyName}' e '{endDateTimePropertyName}' são iguais!", startDateTimePropertyName, endDateTimePropertyName);

        if (startDateTime > endDateTime)
            Errors.RegisterError(GenericErrors.StartBiggerThanEndDateTimeError, $"'{startDateTimePropertyName}' é maior que '{endDateTimePropertyName}'", startDateTimePropertyName, endDateTimePropertyName);
    }

    protected void NotBiggerThanNowDateTimeValidation(string propertyName, DateTime dateTime, bool validateSameDateTimes = false)
    {
        DateTime utcNow = DateTime.UtcNow;

        if (validateSameDateTimes && dateTime == utcNow)
            Errors.RegisterError(GenericErrors.SameDateTimeError, $"'{propertyName}' e '{nameof(utcNow)}' são iguais!", propertyName, nameof(utcNow));

        if (dateTime > utcNow)
            Errors.RegisterError(GenericErrors.DateTimeBiggerThanNowError, $"'{propertyName}' é maior que '{nameof(utcNow)}'", propertyName, nameof(utcNow));
    }

    protected void IpValidation(string propertyName, string ip)
    {
        if (string.IsNullOrEmpty(ip))
            Errors.RegisterError(GenericErrors.InvalidIpError, $"'{propertyName}' está vazio.");

        string[] ipFields = ip.Split('.');

        if (ipFields.Length != 4)
            Errors.RegisterError(GenericErrors.InvalidIpError, $"'{propertyName}' não é um IP válido. Não possui os 4 campos de IP.");

        foreach (string ipField in ipFields)
        {
            if (!byte.TryParse(ipField, out _))
            {
                Errors.RegisterError(GenericErrors.InvalidIpError, $"'{propertyName}' possui um ou mais valores inválidos.");
                return;
            }
        }
    }
}

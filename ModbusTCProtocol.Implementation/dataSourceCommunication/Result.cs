namespace Domain.Core.Concrete
{
    /// <summary>
    /// Modela el resultado de una acción.
    /// </summary>
    public class Result
    {
        protected bool? _isFailure = null;

        #region Properties
        public bool IsSuccess => _isFailure.HasValue && !_isFailure.Value;
        public bool IsFailure => _isFailure.HasValue && _isFailure.Value;
        public ICollection<string> ErrorMessages { get; protected init; } = [];
        public ICollection<Error> Errors { get; protected init; } = [];

        #endregion

        public Result()
        {
        }

        public static Result Success() => new()
        {
            _isFailure = false,
        };

        public static Result Failure(string errorMessage) => new()
        {
            _isFailure = true,
            ErrorMessages = [errorMessage]
        };

        public static Result Failure(IEnumerable<string> errorMessages) => new()
        {
            _isFailure = true,
            ErrorMessages = new List<string>(errorMessages)
        };

        public static Result Failure(Error error) => new()
        {
            _isFailure = true,
            Errors = [error]
        };

        public static Result Failure(IEnumerable<Error> errors) => new()
        {
            _isFailure = true,
            Errors = new List<Error>(errors)
        };

        public static Result Merge(params Result[] results)
        => new()
        {
            _isFailure = !results.All(r => r.IsSuccess),
            Errors = results.SelectMany(x => x.Errors).ToList(),
            ErrorMessages = results.SelectMany(x => x.ErrorMessages).ToList()
        };

        public virtual void WithError(Error error)
        {
            _isFailure = true;
            Errors.Add(error);
        }

        public virtual void WithError(string errorMessage)
        {
            _isFailure = true;
            ErrorMessages.Add(errorMessage);
        }

        public void WithSuccess()
        {
            _isFailure = false;
        }
    }

    public class Result<T> : Result
    {
        public T? Value { get; private set; } = default;

        public Result() : base()
        {
        }

        public static Result<T> Success(T value) => new()
        {
            _isFailure = false,
            Value = value
        };

        public static new Result<T> Failure(string errorMessage) => new()
        {
            _isFailure = true,
            ErrorMessages = [errorMessage]
        };

        public static new Result<T> Failure(IEnumerable<string> errorMessages) => new()
        {
            _isFailure = true,
            ErrorMessages = new List<string>(errorMessages)
        };

        public static new Result<T> Failure(Error error) => new()
        {
            _isFailure = true,
            Errors = [error]
        };

        public static new Result<T> Failure(IEnumerable<Error> errors) => new()
        {
            _isFailure = true,
            Errors = new List<Error>(errors)
        };

        public override void WithError(Error error)
        {
            base.WithError(error);
            Value = default;
        }

        public override void WithError(string errorMessage)
        {
            base.WithError(errorMessage);
            Value = default;
        }

        public void WithSuccess(T value)
        {
            WithSuccess();
            Value = value;
        }
    }
}

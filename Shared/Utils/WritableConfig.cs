using Fibertest.Utils.Setup;
using Newtonsoft.Json;

namespace Fibertest.Utils
{
    public interface IWritableConfig<T> where T : class, new()
    {
        T Value { get; set; }
        void Update(Action<T> applyChanges);
    }

    public class WritableConfig<T> : IWritableConfig<T> where T : class, new()
    {
        private readonly string _fullFilename;
        public T Value { get; set; }

        public WritableConfig(string filename)
        {
            _fullFilename = FileOperations.GetMainFolder() + $"/config/{filename}";
            ConfigUtils.EnsureCreation<T>(_fullFilename);
      
            Value = JsonConvert.DeserializeObject<T>(File.ReadAllText(_fullFilename)) ?? new T();
        }

        public void Update(Action<T> applyChanges)
        {
            applyChanges(Value);
            File.WriteAllText(_fullFilename, JsonConvert.SerializeObject(Value, Formatting.Indented));
        }
    }
}

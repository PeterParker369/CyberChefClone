using CyberChefClone.Operations;

namespace CyberChefClone.Services;

public class OperationRegistry
{
    private readonly List<IOperation> _operations = new();

    public OperationRegistry()
    {
        Console.WriteLine("🚀 Registering operations...");

        // All operations
        _operations.Add(new Base64EncodeOperation());
        _operations.Add(new Base64DecodeOperation());
        _operations.Add(new XorOperation());
        _operations.Add(new Sha256Operation());
        _operations.Add(new HashOperation());
        _operations.Add(new ReverseOperation());
        _operations.Add(new ToUpperOperation());
        _operations.Add(new ToLowerOperation());
        _operations.Add(new ConvertEncodingOperation());
        _operations.Add(new AesEncryptOperation());
        _operations.Add(new AesDecryptOperation());
        _operations.Add(new ToHexOperation());
        _operations.Add(new FromHexOperation());
        _operations.Add(new Pbkdf2Operation());
        _operations.Add(new DeflateOperation());
        _operations.Add(new InflateOperation());

        Console.WriteLine($"✅ Registered {_operations.Count} operations");
        foreach (var op in _operations)
        {
            Console.WriteLine($"  - {op.Name}");
        }
    }

    public List<OperationInfo> GetAllOperations()
    {
        var result = new List<OperationInfo>();

        foreach (var operation in _operations)
        {
            try
            {
                result.Add(new OperationInfo
                {
                    Name = operation.Name,
                    Description = operation.Description,
                    Category = operation.Category,
                    Parameters = operation.ParameterTypes.Select(p => new ParameterInfoDto
                    {
                        Name = p.Key,
                        Type = p.Value.Type,
                        Label = p.Value.Label,
                        DefaultValue = p.Value.DefaultValue,
                        Options = p.Value.Options,
                        Description = p.Value.Description
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting info for {operation.Name}: {ex.Message}");
            }
        }

        return result;
    }

    public IOperation CreateOperation(string name)
    {
        var operation = _operations.FirstOrDefault(o => o.Name == name);
        if (operation == null)
            throw new ArgumentException($"Operation '{name}' not found");

        return operation;
    }
}
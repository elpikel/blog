Last week I've been working on test generator project, where I had to implement 3 patterns of test: AbstractClass, SqlCommand and DelegateMethod. During implementation I had to deal with particular problem that I has to solve. Each class inherit from other but it is not always a case and to get rid of all if checkers if class has base class I used Null Object Pattern.

Here is code that I noticed in solution:

```csharp
public bool HasBaseAbstractClass
{
    get
    {
        if (ClassInheritingDictionary.Count > 0)
        {
            foreach (var inheritedClass in ClassInheritingDictionary)
            {
                if (inheritedClass.Value.IsAbstract)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

public Dictionary<string, CsharpFunction> BasePublicStaticFunctions {
  get {
    if(HasBaseAbstractClass) {
      return BaseClassInfo.BasePublicStaticFunctions
    }

    return new Dictionary<string, CsharpFunction>();
  }
}

```

This pattern was repeated all over the place because many properties depended on this BaseClass check. So to have it only in one place I refactored to following code:

```csharp
 public interface IBaseClassInfo
{
    string BaseClassName { get; }

    string TestClassName { get; }

    string BaseClassDeclaredObjectName { get; }

    CsharpClassModel BaseClass { get; }

    IDictionary<string, IClassMember> BaseProperties { get; }

    Dictionary<string, CsharpFunction> BasePublicFunctions { get; }

    Dictionary<string, CsharpFunction> BasePublicStaticFunctions { get; }
}
```

I introduced this interface so we could depend on abstraction not on the implementation. Next I created two classes that implement this interface:

```csharp
public class BaseClassInfo : IBaseClassInfo
{
    private string className;
    private Dictionary<string, ClassInheritingModel> classInheritingDictionary;

    public BaseClassInfo(string className, Dictionary<string, ClassInheritingModel> classInheritingDictionary)
    {
        this.className = className;
        this.classInheritingDictionary = classInheritingDictionary;
    }

    public string BaseClassName
    {
        get
        {
            return classInheritingDictionary.First().Value.ReferenceOfCsharpClassModel.Name;
        }
    }

    public CsharpClassModel BaseClass
    {
        get
        {
            return classInheritingDictionary.First().Value.ReferenceOfCsharpClassModel;
        }
    }

    public IDictionary<string, IClassMember> BaseProperties
    {
        get
        {
            return classInheritingDictionary.First().Value.ReferenceOfCsharpClassModel.PropertiesList;
        }
    }

    public Dictionary<string, CsharpFunction> BasePublicFunctions
    {
        get
        {
            return classInheritingDictionary.First().Value.ReferenceOfCsharpClassModel.PublicFunctionsList;
        }
    }

    public Dictionary<string, CsharpFunction> BasePublicStaticFunctions
    {
        get
        {
            return BasePublicFunctions.Where(f => f.Value.IsStatic).ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public string TestClassName
    {
        get
        {
            return $"{className}AbstractBase";
        }
    }

    public string BaseClassDeclaredObjectName
    {
        get
        {
            return BaseClass.ClassDeclaredObjectName;
        }
    }
}
```

This class is concrete implementation of above interface and following is null object in case if class does not have base class:

```csharp
public class NullBaseClassInfo : IBaseClassInfo
{
    public NullBaseClassInfo(string className)
    {
        TestClassName = className;
    }

    public string BaseClassName
    {
        get
        {
            return string.Empty;
        }
    }

    public CsharpClassModel BaseClass
    {
        get
        {
            return null;
        }
    }

    public IDictionary<string, IClassMember> BaseProperties
    {
        get
        {
            return new Dictionary<string, IClassMember>();
        }
    }

    public Dictionary<string, CsharpFunction> BasePublicFunctions
    {
        get
        {
            return new Dictionary<string, CsharpFunction>();
        }
    }

    public Dictionary<string, CsharpFunction> BasePublicStaticFunctions
    {
        get
        {
            return new Dictionary<string, CsharpFunction>();
        }
    }

    public string TestClassName { get; }

    public string BaseClassDeclaredObjectName
    {
        get
        {
            return string.Empty;
        }
    }
}
```

This class provides all of the default values for all used behaviours. Finally we can use it in main class:

```csharp
public bool HasBaseAbstractClass
{
    get
    {
        if (ClassInheritingDictionary.Count > 0)
        {
            foreach (var inheritedClass in ClassInheritingDictionary)
            {
                if (inheritedClass.Value.IsAbstract)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

public string BaseClassName => BaseClassInfo.BaseClassName;

public string BaseClassDeclaredObjectName => BaseClassInfo.BaseClassDeclaredObjectName;

public IDictionary<string, IClassMember> BaseProperties => BaseClassInfo.BaseProperties;

public string TestClassName => BaseClassInfo.TestClassName;

public string BaseClassClassDeclaredObjectName => BaseClassInfo.BaseClassDeclaredObjectName;

public Dictionary<string, CsharpFunction> BasePublicStaticFunctions => BaseClassInfo.BasePublicStaticFunctions;

public Dictionary<string, CsharpFunction> BasePublicFunctions => BaseClassInfo.BasePublicFunctions;

private IBaseClassInfo BaseClassInfo
{
    get
    {
        if (HasBaseAbstractClass)
        {
            return new BaseClassInfo(ClassName, ClassInheritingDictionary);
        }

        return new NullBaseClassInfo(ClassName);
    }
}
```

Whenever you observe repetitive checks if some object is null you can use this pattern to reduce amount of if statements.

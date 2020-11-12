# Localization
A library for .net that can help you to manage the localization in your application with a simple method.

# Architecture
The following image represets a localization manager diagram:

![alt Localization Manager Diagram](art/diagram.png)

Actually there are two extenetion of library: Wpf Window and Xamarin App. 
The library can be used without extentions but using the Localization Manager 
Instance directly.

# LocalizationManager

LocalizationManager is more important component of this library. The library manages
the current culture and it provides to load a vocabolary in culture.

### Init

The Init provides to initilize the LocalizationManager. There two kind: Initializing
without culture (take the thread cultire) and with a specific culture.

The initilizing without culture like this:

```c#
LocalizationManager.Init(new MockVocabolaryServiceProvider { });
```

The inizializing with the culture like this:

```c#
LocalizationManager.Init(new MockVocabolaryServiceProvider { }, new CultureInfo("en-US"));
```

The inizializing of library is very important to load the vocabolary.

### SetCulture

```c#
LocalizationManager.SetCulture(new CultureInfo("en-US"));
```

### Translate without default value

```c#
var label = LocalizationManager.Instance["resourceKey"];
```

### Translate with default value

```c#
var label = LocalizationManager.Instance.Translate("resourceKey","#Default value");
```

# Vocabolary Service Provider
line...
```c# xaml
public class MockVocabolaryServiceProvider : IVocabolaryServiceProvider
{
    Dictionary<string, Vocabolary> all = null;

    public Task AddOrUpdateTermAsync(IVocabolary vocabolary, string key, string defaultValue = null) 
        => Task.Delay(1);

    public Task Initialize()
    {
        all = new Dictionary<string, Vocabolary>
        {
            {"it-IT", new Vocabolary {
                    { "MainWindow","Main Window in italian language" }
                }
            },
            {"en-US", new Vocabolary {
                    { "MainWindow","Main Window in english language" } 
                } 
            }
        };
        return Task.FromResult(all);
    }

    public Task<IVocabolary> LoadVocabolaryAsync(CultureInfo cultureInfo)
    {
        string cultureDefault = "it-IT";
        if (all.ContainsKey(cultureInfo.ToString()))
            cultureDefault = cultureInfo.ToString();
        return Task.FromResult<IVocabolary>(all[cultureDefault]);
    }

    public Task SaveAsync(IVocabolary vocabolary) 
        => Task.Delay(1);
}
```

# Localization Uses
line....

# Xamarin
line...

```c#
LocalizationManager.Init(new MockVocabolaryServiceProvider { });
```

```c# xaml
 xmlns:culture="clr-namespace:Localization.Xamarin;assembly=Localization.Xamarin"
```

```c# xaml
<Label Text="{culture:Translate MainWindow, DefaultValue=Main Window}"></Label>
```

# WPF
line...

```c#
LocalizationManager.Init(new MockVocabolaryServiceProvider { });
```

```c# xaml
xmlns:culture="clr-namespace:Localization.Windows;assembly=Localization.Windows"
```

```c# xaml
<Label Content="{culture:Translate MainWindow, DefaultValue=Main Window}"></Label>
```

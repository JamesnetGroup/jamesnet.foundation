using Xunit;

namespace Jamesnet.Foundation.Tests;

public class ContainerTests
{
    public interface IService { }
    public class Service : IService { }
    public class Consumer { public IService Service { get; } public Consumer() { } public Consumer(IService service) => Service = service; }
    public class CycleA { public CycleA(CycleB b) { } }
    public class CycleB { public CycleB(CycleA a) { } }
    public class NoPublicCtor { private NoPublicCtor() { } }

    [Fact]
    public void Register_creates_new_instance_each_time()
    {
        var c = new Container();
        c.Register<IService, Service>();
        Assert.NotSame(c.Resolve<IService>(), c.Resolve<IService>());
    }

    [Fact]
    public void RegisterSingleton_returns_same_instance()
    {
        var c = new Container();
        c.RegisterSingleton<IService, Service>();
        Assert.Same(c.Resolve<IService>(), c.Resolve<IService>());
    }

    [Fact]
    public void Named_registrations_are_separate()
    {
        var c = new Container();
        var a = new Service();
        var b = new Service();
        c.RegisterInstance<IService>(a, "a");
        c.RegisterInstance<IService>(b, "b");
        Assert.Same(a, c.Resolve<IService>("a"));
        Assert.Same(b, c.Resolve<IService>("b"));
    }

    [Fact]
    public void Uses_constructor_with_most_parameters()
    {
        var c = new Container();
        c.RegisterSingleton<IService, Service>();
        Assert.Same(c.Resolve<IService>(), c.Resolve<Consumer>().Service);
    }

    [Fact]
    public void Circular_dependency_throws_instead_of_stack_overflow()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new Container().Resolve<CycleA>());
        Assert.Contains("Circular", ex.Message);
    }

    [Fact]
    public void Circular_check_resets_after_failure()
    {
        var c = new Container();
        Assert.Throws<InvalidOperationException>(() => c.Resolve<CycleA>());
        Assert.NotNull(c.Resolve<Service>());
    }

    [Fact]
    public void Type_without_public_constructor_throws_clear_error()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new Container().Resolve<NoPublicCtor>());
        Assert.Contains("no public constructor", ex.Message);
    }

    [Fact]
    public void Unregistered_interface_throws_and_TryResolve_returns_false()
    {
        var c = new Container();
        Assert.Throws<InvalidOperationException>(() => c.Resolve<IService>());
        Assert.False(c.TryResolve<IService>(out var result));
        Assert.Null(result);
    }
}

public class RelayCommandTests
{
    [Fact]
    public void Generic_command_with_value_type_accepts_null_parameter()
    {
        int? seen = null;
        var cmd = new RelayCommand<int>(v => seen = v, v => v == 0);
        Assert.True(cmd.CanExecute(null));
        cmd.Execute(null);
        Assert.Equal(0, seen);
    }

    [Fact]
    public void Generic_command_passes_typed_parameter()
    {
        string seen = null;
        var cmd = new RelayCommand<string>(v => seen = v);
        cmd.Execute("hello");
        Assert.Equal("hello", seen);
    }

    [Fact]
    public void CanExecute_uses_predicate()
    {
        bool allowed = false;
        var cmd = new RelayCommand(() => { }, () => allowed);
        Assert.False(cmd.CanExecute(null));
        allowed = true;
        Assert.True(cmd.CanExecute(null));
    }

    [Fact]
    public void Null_execute_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new RelayCommand(null));
        Assert.Throws<ArgumentNullException>(() => new RelayCommand<int>(null));
    }
}

public class ViewModelBaseTests
{
    private class Vm : ViewModelBase
    {
        private string _name;

        public Vm() => SaveCommand = new RelayCommand(() => { }, () => !string.IsNullOrEmpty(Name));

        [CommandTrigger(nameof(SaveCommand))]
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        public RelayCommand SaveCommand { get; }

        public void RaiseAll() => OnPropertyChanged(null);
    }

    [Fact]
    public void SetProperty_raises_PropertyChanged_only_on_change()
    {
        var vm = new Vm();
        var raised = new List<string>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.Name = "a";
        vm.Name = "a";

        Assert.Equal(new[] { "Name" }, raised);
    }

    [Fact]
    public void CommandTrigger_raises_CanExecuteChanged()
    {
        var vm = new Vm();
        int count = 0;
        vm.SaveCommand.CanExecuteChanged += (_, _) => count++;

        vm.Name = "a";

        Assert.Equal(1, count);
        Assert.True(vm.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void Raising_for_all_properties_does_not_throw()
    {
        new Vm().RaiseAll();
    }
}

public class LayerManagerTests
{
    private class FakeLayer : ILayer
    {
        public string LayerName { get; set; }
        public bool IsRegistered { get; set; }
        public object UIContent { get; set; }
    }

    private class FakeView : IView
    {
        public object DataContext { get; set; }
    }

    private class Vm : IViewActivated, IViewClosed, IViewLoadable
    {
        public List<string> Log { get; } = new();
        public void ViewActivated(object view) => Log.Add("activated");
        public void OnClosed(object view) => Log.Add("closed");
        public void OnLoaded(object view) => Log.Add("loaded");
    }

    [Fact]
    public void Show_replaces_content_and_notifies_view_models()
    {
        var manager = new LayerManager();
        var layer = new FakeLayer();
        manager.Register("main", layer);

        var vm1 = new Vm();
        var vm2 = new Vm();
        var view1 = new FakeView { DataContext = vm1 };
        var view2 = new FakeView { DataContext = vm2 };

        manager.Show("main", view1);
        manager.Show("main", view2);

        Assert.Same(view2, layer.UIContent);
        Assert.Equal(new[] { "activated", "closed" }, vm1.Log);
        Assert.Equal(new[] { "activated" }, vm2.Log);
    }

    [Fact]
    public void Showing_same_view_again_only_reloads()
    {
        var manager = new LayerManager();
        manager.Register("main", new FakeLayer());
        var vm = new Vm();
        var view = new FakeView { DataContext = vm };

        manager.Show("main", view);
        manager.Show("main", view);

        Assert.Equal(new[] { "activated", "loaded" }, vm.Log);
    }

    [Fact]
    public void Unknown_layer_throws()
    {
        Assert.Throws<InvalidOperationException>(() => new LayerManager().Show("nope", new FakeView()));
    }

    [Fact]
    public void Reregistering_moves_mapped_view_to_new_layer()
    {
        var manager = new LayerManager();
        var view = new FakeView();
        manager.Mapping("main", view);

        manager.Register("main", new FakeLayer());
        var second = new FakeLayer();
        manager.Register("main", second);

        Assert.Same(second, manager.GetLayer("main"));
        Assert.Same(view, second.UIContent);
    }

    [Fact]
    public void Hide_clears_content()
    {
        var manager = new LayerManager();
        var layer = new FakeLayer();
        manager.Register("main", layer);
        manager.Show("main", new FakeView());

        manager.Hide("main");

        Assert.Null(layer.UIContent);
    }
}

public class YamlTests
{
    private const string Sample = """
        # comment
        - name: Alpha
          ratio: 1.5
          count: 3
        - name: Beta
          url: http://example.com:8080/path
        """;

    [Fact]
    public void Parses_items_and_keeps_colons_in_values()
    {
        var items = YamlConverter.Parse(Sample).ToList();

        Assert.Equal(2, items.Count);
        Assert.Equal("Alpha", items[0]["name"]);
        Assert.Equal("http://example.com:8080/path", items[1]["url"]);
    }

    [Fact]
    public void GetValue_is_culture_invariant()
    {
        var previous = Thread.CurrentThread.CurrentCulture;
        try
        {
            // 소수점이 쉼표인 문화권에서도 "1.5"를 1.5로 읽어야 한다.
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            var item = YamlConverter.Parse(Sample).First();

            Assert.Equal(1.5, item.GetValue<double>("ratio"));
            Assert.Equal(1.5, new YamlItem(item.ToDictionary(p => p.Key, p => p.Value)).GetValue<double>("ratio"));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = previous;
        }
    }

    [Fact]
    public void GetValue_returns_default_for_missing_or_invalid()
    {
        var item = YamlConverter.Parse(Sample).First();

        Assert.Equal(7, item.GetValue("missing", 7));
        Assert.Equal(9, item.GetValue("name", 9));
        Assert.Equal(3, item.GetValue<int>("count"));
    }

    [Fact]
    public void Missing_resource_throws_FileNotFound()
    {
        Assert.Throws<System.IO.FileNotFoundException>(() => YamlConverter.ParseResource(typeof(YamlTests).Assembly, "nope.yml"));
    }
}

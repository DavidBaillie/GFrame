using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manager is responsible for holding onto resource loading events and invoking callbacks when they complete
/// </summary>
[GlobalClass]
public partial class AsyncResourceLoadingManager : Node
{
    public static AsyncResourceLoadingManager Instance
    {
        get; set;
    }

    private System.Collections.Generic.Dictionary<string, LoadRequest> loadingTasks = new();

    /// <summary>
    /// Set singleton
    /// </summary>
    public override void _Ready()
    {
        base._Ready();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.PrintErr($"A duplicate {nameof(AsyncResourceLoadingManager)} was detected!");
        }
    }

    /// <summary>
    /// Updates each frame to process the state of a loading task
    /// </summary>
    public override void _Process(double delta)
    {
        base._Process(delta);

        // Clear when we're done
        System.Collections.Generic.List<string> toRemove = new();

        // State per task
        Godot.Collections.Array progress = new();

        // For all the tasks
        foreach (var task in loadingTasks)
        {
            try
            {
                var statuses = task.Value.Paths.Select(x =>
                {
                    var status = ResourceLoader.LoadThreadedGetStatus(x, progress);
                    return new LoadingStatus()
                    {
                        Path = x,
                        Status = status,
                        Percent = status == ResourceLoader.ThreadLoadStatus.InProgress ? progress[0].AsDouble() : 0
                    };
                });


                // Any loading?
                if (statuses.Any(x => x.Status == ResourceLoader.ThreadLoadStatus.InProgress))
                {
                    task.Value.OnLoadingInProgress?.Invoke(statuses.Sum(x => x.Percent) / statuses.Count());
                }
                // All loaded, any failures?
                else if (statuses.Any(x => x.Status == ResourceLoader.ThreadLoadStatus.Failed || x.Status == ResourceLoader.ThreadLoadStatus.InvalidResource))
                {
                    var loaded = statuses.Where(x => x.Status == ResourceLoader.ThreadLoadStatus.Loaded)
                        .Select(x => ResourceLoader.LoadThreadedGet(x.Path))
                        .ToList();
                    var failed = statuses.Where(x => x.Status != ResourceLoader.ThreadLoadStatus.Loaded)
                        .Select(x => x.Status)
                        .ToList();

                    toRemove.Add(task.Key);

                    task.Value.OnComplete?.Invoke(loaded, failed);
                }
                // All loaded and good
                else
                {
                    toRemove.Add(task.Key);
                    task.Value.OnComplete?.Invoke(statuses.Select(x => ResourceLoader.LoadThreadedGet(x.Path)).ToList(), new());
                }
            }
            catch (Exception ex)
            {
                // Hope we never hit this
                GD.PrintErr($"Failed to parse a resource load status: {task.Key}\n{ex}");
                toRemove.Add(task.Key);
            }
        }

        // Remove invalid tasks
        toRemove.ForEach(x => loadingTasks.Remove(x));
    }

    /// <summary>
    /// Add a new resource load to the 
    /// </summary>
    /// <param name="key">Key to remember item by</param>
    /// <param name="path">Path to resource</param>
    /// <param name="OnCompleteLoading">Invoked when loading is complete</param>
    /// <param name="OnLoadingInProgress">Invoked each frame the loading is progressing</param>
    /// <param name="OnError">Invoked if the load fails</param>
    public static void AddResourceLoadEvent(string key, string path, Action<List<Resource>, List<ResourceLoader.ThreadLoadStatus>> OnCompleteLoading,
        Action<double> OnLoadingInProgress = null, Action<Error> OnError = null)
    {
        // Clear last request
        if (Instance.loadingTasks.ContainsKey(key))
            Instance.loadingTasks.Remove(key);

        var error = ResourceLoader.LoadThreadedRequest(path);
        if (error != Error.Ok)
        {
            OnError?.Invoke(error);
            return;
        }

        var request = new LoadRequest()
        {
            Paths = new List<string> { path },
            OnComplete = OnCompleteLoading,
            OnLoadingInProgress = OnLoadingInProgress,
            OnError = OnError
        };

        Instance.loadingTasks.Add(key, request);
    }

    /// <summary>
    /// Add a new resource load to the 
    /// </summary>
    /// <param name="key">Key to remember item by</param>
    /// <param name="path">Path to resource</param>
    /// <param name="OnCompleteLoading">Invoked when loading is complete</param>
    /// <param name="OnLoadingInProgress">Invoked each frame the loading is progressing</param>
    /// <param name="OnError">Invoked if the load fails</param>
    public static void AddResourcesLoadEvent(string key, List<string> paths, Action<List<Resource>, List<ResourceLoader.ThreadLoadStatus>> OnCompleteLoading,
        Action<double> OnLoadingInProgress = null, Action<Error> OnError = null)
    {
        // Clear last request
        if (Instance.loadingTasks.ContainsKey(key))
            Instance.loadingTasks.Remove(key);

        foreach (var path in paths)
        {
            var error = ResourceLoader.LoadThreadedRequest(path);
            if (error != Error.Ok)
            {
                OnError.Invoke(error);
                return;
            }
        }


        Instance.loadingTasks.Add(key, new()
        {
            Paths = paths,
            OnComplete = OnCompleteLoading,
            OnLoadingInProgress = OnLoadingInProgress,
            OnError = OnError
        });
    }

    private struct LoadRequest
    {
        public List<string> Paths { get; set; }
        public Action<List<Resource>, List<ResourceLoader.ThreadLoadStatus>> OnComplete { get; set; }
        public Action<double> OnLoadingInProgress { get; set; }
        public Action<Error> OnError { get; set; }
    }

    private struct LoadingStatus
    {
        public string Path { get; set; }
        public ResourceLoader.ThreadLoadStatus Status { get; set; }
        public double Percent { get; set; }
    }
}

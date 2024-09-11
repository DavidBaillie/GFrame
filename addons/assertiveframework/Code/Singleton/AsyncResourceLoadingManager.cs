using Godot;
using System;

/// <summary>
/// Manager is responsible for holding onto resource loading events and invoking callbacks when they complete
/// </summary>
[GlobalClass]
public partial class AsyncResourceLoadingManager : Node
{
    public static AsyncResourceLoadingManager Instance { get; private set; }

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
        ResourceLoader.ThreadLoadStatus status;
        Godot.Collections.Array progress = new();

        // For all the tasks
        foreach (var task in loadingTasks)
        {
            try
            {
                // Get a status
                status = ResourceLoader.LoadThreadedGetStatus(task.Value.Path, progress);

                // Handle status state
                switch (status)
                {
                    case ResourceLoader.ThreadLoadStatus.Failed:
                        task.Value.OnError?.Invoke(Error.Failed);
                        toRemove.Add(task.Key);
                        break;

                    case ResourceLoader.ThreadLoadStatus.Loaded:
                        task.Value.OnComplete?.Invoke(ResourceLoader.LoadThreadedGet(task.Value.Path));
                        toRemove.Add(task.Key);
                        break;

                    case ResourceLoader.ThreadLoadStatus.InProgress:
                        task.Value.OnLoadingInProgress?.Invoke(progress.Count > 0 ? progress[0].AsDouble() : 0);
                        break;

                    case ResourceLoader.ThreadLoadStatus.InvalidResource:
                        task.Value.OnError?.Invoke(Error.FileNotFound);
                        toRemove.Add(task.Key);
                        break;
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
    public static void AddResourceLoadEvent(string key, string path, Action<Resource> OnCompleteLoading, Action<double> OnLoadingInProgress = null, Action<Error> OnError = null)
    {
        // Clear last request
        if (Instance.loadingTasks.ContainsKey(key))
            Instance.loadingTasks.Remove(key);

        var error = ResourceLoader.LoadThreadedRequest(path);
        if(error != Error.Ok)
        {
            OnError?.Invoke(error);
            return;
        }

        Instance.loadingTasks.Add(key, new()
        {
            Path = path,
            OnComplete = OnCompleteLoading,
            OnLoadingInProgress = OnLoadingInProgress,
            OnError = OnError
        });
    }

    private struct LoadRequest
    {
        public string Path { get; set; }
        public Action<Resource> OnComplete { get; set; }
        public Action<double> OnLoadingInProgress { get; set; }
        public Action<Error> OnError { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TF2_RenderDemo_Batcher.Models;

namespace TF2_RenderDemo_Batcher
{
    class Program
    {

        static void Main(string[] args)
        {
            SettingsFile settings = JsonSerializer.Deserialize<SettingsFile>(File.ReadAllText(".\\settings.json"));
            Dictionary<string, DemoEventLog> demoEventLogs = DeserializeDemoEventLogsInDirectory(settings.demoDirectory);
            RenderDemoOptions renderDemoOptions = new RenderDemoOptions
            {
                hl2Path = settings.hl2Path,
                launchOptions = settings.launchOptions,
                overwrite = settings.overwrite,
                exportPath = settings.exportPrefix,
                profile = settings.profile,
                preRecordCommand = settings.preRecordCommand,
                windowState = settings.windowState
            };
            var commandList = BuildCommandList(renderDemoOptions, demoEventLogs);
            string exportString = "";
            foreach (string command in commandList)
            {
                exportString += command + "\n";
            }
            File.WriteAllText($".\\RenderDemo-Batcher_{DateTime.Now.ToFileTime()}.bat", exportString);
        }

        /// <summary>
        /// Builds the command string that renders a demo.
        /// </summary>
        /// <returns>A string usable to launch RenderDemo.</returns>
        private static string BuildCommandString(RenderDemoOptions renderDemoOptions)
        {
            string commandString = $"renderdemo -exepath \"{renderDemoOptions.hl2Path}\" -demo \"{renderDemoOptions.demoPath}\" -start {renderDemoOptions.startTick} -end {renderDemoOptions.endTick} -out \"{renderDemoOptions.exportPath}\"";
            if (!String.IsNullOrWhiteSpace(renderDemoOptions.launchOptions))
            {
                commandString += $" -launch \"{renderDemoOptions.launchOptions}\"";
            }
            if (!String.IsNullOrWhiteSpace(renderDemoOptions.preRecordCommand))
            {
                commandString += $" -cmd \"{renderDemoOptions.preRecordCommand}\"";
            }
            return commandString;
        }

        private static List<string> BuildCommandList(RenderDemoOptions renderDemoOptions, Dictionary<string, DemoEventLog> demoEventLogs, int startTickOffset = -990, int ticksPerKill = 990, int tickEndOffset = 990, int clipContinuationTolerance = 1980)
        {
            List<string> commandList = new List<string>();
            foreach (KeyValuePair<string, DemoEventLog> demoEventLogKv in demoEventLogs)
            {
                string path = demoEventLogKv.Key;
                DemoEventLog demoEventLog = demoEventLogKv.Value;
                if (demoEventLog.events.Count > 0)
                {
                    List<Tuple<int, int>> ClipSpans = new List<Tuple<int, int>>();
                    int currentSpanStartTick = 0;
                    int currentSpanEndTick = 0;
                    for (int i = 0; i < demoEventLog.events.Count; i++)
                    {
                        if (i == 0 || currentSpanStartTick == -1)
                        {
                            // Don't go below zero, duh.
                            currentSpanStartTick = Math.Clamp(
                                demoEventLog.events[i].tick
                                + startTickOffset
                                - (ticksPerKill * demoEventLog.events[i].value_i)
                                , 0, Int32.MaxValue);
                        }
                        else
                        {
                            // If the start of this event is close enough to the end of the last event, record as one clip.
                            int thisEventStartTick = (demoEventLog.events[i].tick + startTickOffset - (ticksPerKill * demoEventLog.events[i].value_i));
                            int previousEventMaxTick = (demoEventLog.events[i - 1].tick + tickEndOffset + clipContinuationTolerance);
                            if (thisEventStartTick <= previousEventMaxTick)
                            {
                                currentSpanEndTick = demoEventLog.events[i].tick + tickEndOffset;
                            }
                            else
                            {
                                currentSpanEndTick = demoEventLog.events[i - 1].tick + tickEndOffset;
                                ClipSpans.Add(new Tuple<int, int>(currentSpanStartTick, currentSpanEndTick));
                                currentSpanStartTick = -1;
                            }
                        }
                    }
                    for (int i = 0; i < ClipSpans.Count; i++)
                    {
                        Tuple<int, int> clipSpan = (Tuple<int, int>)ClipSpans[i];
                        RenderDemoOptions clipDemoOptions = new RenderDemoOptions
                        {
                            startTick = clipSpan.Item1,
                            endTick = clipSpan.Item2,
                            demoPath = path,
                            exportPath = Path.Combine(renderDemoOptions.exportPath, Path.GetFileNameWithoutExtension(path) + $"_{i}.avi"),
                            hl2Path = renderDemoOptions.hl2Path,
                            launchOptions = renderDemoOptions.launchOptions,
                            overwrite = renderDemoOptions.overwrite,
                            preRecordCommand = renderDemoOptions.preRecordCommand,
                            profile = renderDemoOptions.profile,
                            windowState = renderDemoOptions.windowState
                        };
                        string commandString = BuildCommandString(clipDemoOptions);
                        commandList.Add(commandString);
                    }
                }
            }
            return commandList;
        }

        /// <summary>
        /// Deserializes demo event JSON logs in a directory.
        /// </summary>
        /// <param name="dirPath">Path to demo directory.</param>
        /// <param name="recursive">Should the demo folder be searched recursively?</param>
        /// <returns>All DemoEventLogs that could be deserialzed.</returns>
        private static Dictionary<string, DemoEventLog> DeserializeDemoEventLogsInDirectory(string dirPath, bool recursive = false)
        {
            var JsonPaths = Directory.EnumerateFiles(dirPath, "*.json", new EnumerationOptions() { RecurseSubdirectories = recursive });
            Dictionary<string, DemoEventLog> demoEventLogs = new Dictionary<string, DemoEventLog>();
            foreach (string path in JsonPaths)
            {
                try
                {
                    string fileContent = File.ReadAllText(path);
                    DemoEventLog DemoEventLog = JsonSerializer.Deserialize<DemoEventLog>(fileContent);
                    demoEventLogs.Add(path.Replace(".json", ""), DemoEventLog);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"There was an error deserializing '{path}'. {ex.Message}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"There was an error opening '{path}' for reading. {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return demoEventLogs;
        }
    }
}

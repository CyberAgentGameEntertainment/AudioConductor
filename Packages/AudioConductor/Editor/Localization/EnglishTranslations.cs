// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;

namespace AudioConductor.Editor.Localization
{
    internal static class EnglishTranslations
    {
        public static Dictionary<string, string> Table { get; } = new()
        {
            { "settings.throttle_type", "Concurrent play control type." },
            { "settings.throttle_limit", "Limit of concurrent play." },
            { "settings.master_volume", "Master volume scale applied to all audio. (0.0 to 1.0)" },
            { "settings.managed_pool_size", "Number of managed AudioClipPlayers to pre-create on construction." },
            { "settings.oneshot_pool_size", "Number of one-shot AudioClipPlayers to pre-create on construction." },
            { "category.name", "Category name." },
            { "category.throttle_type", "Concurrent play control type." },
            { "category.throttle_limit", "Limit of concurrent play." },
            { "category.audio_mixer_group", "Output AudioMixerGroup." },
            { "cue_sheet.tab_parameter", "Edit CueSheet parameter" },
            { "cue_sheet.tab_cue_list", "Edit Cue&Track" },
            { "cue_sheet.tab_other_operation", "Other operations" },
            { "cue_list.toggle_volume", "Show/Hide \"Volume\" & \"Volume Range\" columns" },
            { "cue_list.toggle_play_info", "Show/Hide \"Category\" & \"Play Type\" columns" },
            { "cue_list.toggle_throttle", "Show/Hide \"Throttle Type\" & \"Throttle Limit\" columns" },
            { "cue_list.toggle_memo", "Show/Hide \"Color\" column" },
            { "cue_list.toggle_inspector", "Show/Hide inspector" },
            { "cue_list.column_name", "Cue or Track name." },
            { "cue_list.column_color", "Color label assigned to the Cue or Track." },
            { "cue_list.column_category", "Category assigned to the Cue." },
            { "cue_list.column_throttle_type", "Concurrent play control type for the Cue." },
            { "cue_list.column_throttle_limit", "Limit of concurrent play for the Cue." },
            { "cue_list.column_volume", "Volume scale for the Cue or Track. (0.0 to 1.0)" },
            { "cue_list.column_volume_range", "Random volume variation width for the Cue or Track. (0.0 to 1.0)" },
            { "cue_list.column_play_type", "Cue playback rule for selecting Track order." },
            { "cue_inspector.name", "Cue name." },
            { "cue_inspector.cue_id", "Cue ID." },
            { "cue_inspector.color", "Cue color label." },
            { "cue_inspector.category", "Category applied to this cue." },
            { "cue_inspector.throttle_type", "Concurrent play control type for this cue." },
            { "cue_inspector.throttle_limit", "Limit of concurrent play for this cue." },
            { "cue_inspector.volume", "Cue volume scale. (0.0 to 1.0)" },
            { "cue_inspector.volume_range", "Random volume variation width. (0.0 to 1.0)" },
            { "cue_inspector.pitch", "Cue pitch scale. (0.01 to 3.0)" },
            { "cue_inspector.pitch_range", "Random pitch variation width. (0.0 to 1.0)" },
            { "cue_inspector.pitch_invert", "Invert pitch variation direction." },
            { "cue_inspector.play_type", "Cue playback mode." },
            { "cue_inspector.play", "Play the selected cue using its play type and track selection rules." },
            { "cue_inspector.pause", "Pause or resume cue preview." },
            { "cue_inspector.stop", "Stop cue preview." },
            { "track_inspector.name", "Track name." },
            { "track_inspector.color", "Track color label." },
            { "track_inspector.audio_clip", "AudioClip assigned to this track." },
            { "track_inspector.volume", "Track volume scale. (0.0 to 1.0)" },
            { "track_inspector.volume_range", "Random volume variation width. (0.0 to 1.0)" },
            { "track_inspector.pitch", "Track pitch scale. (0.01 to 3.0)" },
            { "track_inspector.pitch_range", "Random pitch variation width. (0.0 to 1.0)" },
            { "track_inspector.pitch_invert", "Invert pitch variation direction." },
            { "track_inspector.random_weight", "Weight used for random track selection." },
            { "track_inspector.priority", "Playback priority. Smaller values are higher priority." },
            { "track_inspector.fade_time", "Fade time in seconds." },
            { "track_inspector.start_sample", "Playback start sample." },
            { "track_inspector.end_sample", "Playback end sample." },
            { "track_inspector.loop", "Enable looping for this track." },
            { "track_inspector.loop_start_sample", "Sample position where looping restarts." },
            { "track_inspector.analyze", "Analyze the AudioClip and set loop metadata from wav chunks." },
            { "track_inspector.play", "Preview the selected track." },
            { "track_inspector.pause", "Pause or resume track preview." },
            { "track_inspector.stop", "Stop track preview." },
            { "cue_sheet_parameter.name", "CueSheet name." },
            { "cue_sheet_parameter.throttle_type", "Default concurrent play control type for cues in this CueSheet." },
            { "cue_sheet_parameter.throttle_limit", "Default limit of concurrent play for cues in this CueSheet." },
            { "cue_sheet_parameter.volume", "Default CueSheet volume scale. (0.0 to 1.0)" },
            { "cue_sheet_parameter.pitch", "Default CueSheet pitch scale. (0.01 to 3.0)" },
            { "cue_sheet_parameter.pitch_invert", "Invert CueSheet pitch variation direction." },
            { "other_operation.export_csv", "Export CueSheet data to CSV." },
            { "other_operation.import_csv", "Import CueSheet data from CSV." },
            { "cue_enum_definition.default_output_path", "Default output directory for generated enum files." },
            {
                "cue_enum_definition.default_namespace",
                "Default namespace for generated enums. Empty means no namespace."
            },
            {
                "cue_enum_definition.default_class_suffix",
                "Default suffix appended to enum type names. e.g. BGM + Cues = BGMCues."
            },
            { "cue_enum_definition.generate", "Generate enum files from the current definition." },
            { "cue_enum_definition.file_entry.file_name", "Output file name (without extension)." },
            {
                "cue_enum_definition.file_entry.use_default_output_path",
                "Use the default output path from definition settings."
            },
            { "cue_enum_definition.file_entry.output_path", "Output directory for this file group." },
            {
                "cue_enum_definition.file_entry.use_default_namespace",
                "Use the default namespace from definition settings."
            },
            { "cue_enum_definition.file_entry.namespace", "Namespace for this file group." },
            {
                "cue_enum_definition.file_entry.use_default_class_suffix",
                "Use the default class suffix from definition settings."
            },
            { "cue_enum_definition.file_entry.class_suffix", "Suffix appended to enum type names in this file group." },
            {
                "cue_enum_definition.file_entry.path_rule",
                "Glob pattern to auto-assign CueSheetAssets to this file group."
            },
            { "cue_enum_definition.asset.asset", "The CueSheetAsset reference." },
            { "cue_enum_definition.asset.cue_sheet_name", "Name of the CueSheet." },
            { "cue_enum_definition.asset.cue_count", "Number of cues in this CueSheet." }
        };
    }
}

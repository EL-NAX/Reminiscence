using Godot;
using System;
using System.Collections.Generic;

[GlobalClass] // Agar muncul di menu "Create New Resource"
public partial class BlipVoice : Resource
{
	[Export] public Godot.Collections.Array<AudioStream> Sounds { get; set; } = new();
	[Export(PropertyHint.Range, "1,4")] public int Every { get; set; } = 2;
	[Export(PropertyHint.Range, "0.0,12.0")] public float Spread { get; set; } = 1.0f;
	[Export(PropertyHint.Range, "0.25,4.0")] public float Pitch { get; set; } = 1.0f;
	[Export(PropertyHint.Range, "-40.0,12.0")] public float VolumeDb { get; set; } = 0.0f;
	[Export(PropertyHint.Range, "0.0,12.0")] public float QuestionLift { get; set; } = 2.0f;
	[Export] public bool ByLetter { get; set; } = true;

	// Fungsi untuk memilih suara berdasarkan huruf
	public (AudioStream stream, float pitchScale) Pick(string letter, float lift = 0.0f)
	{
		if (Sounds.Count == 0) return (null, 1.0f);

		int code = letter.ToLower()[0]; // Mengambil nilai ASCII/Unicode
		int index = ByLetter ? (code % Sounds.Count) : (int)(GD.Randi() % Sounds.Count);
		
		float step = ByLetter 
			? (float)((code * 7) % 5 - 2) / 2.0f 
			: (float)GD.RandRange(-1.0, 1.0);

		float pitchScale = Pitch * Mathf.Pow(2.0f, (step * Spread + lift) / 12.0f);

		return (Sounds[index], pitchScale);
	}
}

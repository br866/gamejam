/*
Generate_RefugeFeedback.scd

Renders two related UI/world-feedback one-shots for Don't Leave Me:
  - SFX_SafeZone_Activate_SC.wav
  - SFX_Checkpoint_Activate_SC.wav

Both sounds share a filtered whoosh, soft high shimmer, and understated G/D
resonance. The checkpoint version has a firmer low-mid confirmation.

Run from SuperCollider, or from a terminal:
  sclang Generate_RefugeFeedback.scd

Output: UnityProject_WwiseProject/Originals/SFX
Format: 48 kHz / 24-bit / stereo WAV
*/

(
var scriptPath = thisProcess.nowExecutingPath;
var scriptDir = PathName(scriptPath).pathOnly;
var audioScriptsDir = PathName(scriptDir).parentPath;
var unityProjectDir = PathName(audioScriptsDir).parentPath;
var outputDir = unityProjectDir +/+ "UnityProject_WwiseProject" +/+ "Originals" +/+ "SFX";
var safePath = outputDir +/+ "SFX_SafeZone_Activate_SC.wav";
var checkpointPath = outputDir +/+ "SFX_Checkpoint_Activate_SC.wav";
var options;
var safeDef;
var checkpointDef;
var render;

options = ServerOptions.new;
options.numOutputBusChannels = 2;
options.numInputBusChannels = 0;
options.sampleRate = 48000;
options.blockSize = 64;
options.memSize = 8192;

safeDef = SynthDef(\refugeSafeZone, { |out = 0|
    var master;
    var whooshEnv;
    var whooshCenter;
    var whoosh;
    var shimmerEnv;
    var shimmerExciter;
    var shimmer;
    var toneEnv;
    var tone;
    var dry;
    var wet;
    var signal;

    master = EnvGen.ar(
        Env([0, 1, 1, 0], [0.025, 1.48, 0.22], [2, 0, -5]),
        doneAction: 2
    );

    whooshEnv = EnvGen.ar(Env([0, 1, 0.52, 0], [0.09, 0.46, 0.88], [-2, -1, -5]));
    whooshCenter = EnvGen.kr(Env([260, 1320, 720], [0.42, 0.98], [2, -3]));
    whoosh = BPF.ar(PinkNoise.ar(0.72), whooshCenter, 0.72) * whooshEnv;
    whoosh = whoosh + (HPF.ar(BrownNoise.ar(0.15), 170) * whooshEnv * 0.22);
    whoosh = LPF.ar(whoosh, 4300);

    shimmerEnv = EnvGen.ar(Env([0, 0, 1, 0.42, 0], [0.16, 0.22, 0.42, 0.55], [0, 3, -2, -5]));
    shimmerExciter = Dust2.ar(13) * shimmerEnv * 0.055;
    shimmer = Mix.ar(Ringz.ar(
        shimmerExciter,
        [1567.98, 1760.00, 2093.00, 2349.32, 3135.96],
        [0.075, 0.09, 0.065, 0.085, 0.055]
    ));
    shimmer = LPF.ar(HPF.ar(shimmer, 1150), 6100) * 0.16;

    toneEnv = EnvGen.ar(Env.perc(0.12, 1.18, curve: -5));
    tone = (SinOsc.ar(98.00, 0, 0.035) + SinOsc.ar(146.83, 0.3, 0.014)) * toneEnv;

    dry = Pan2.ar(whoosh * 0.46, -0.08)
        + Pan2.ar(shimmer, 0.2)
        + Pan2.ar(tone, 0);
    wet = FreeVerb2.ar(dry[0], dry[1], mix: 0.36, room: 0.68, damp: 0.74);
    signal = (dry * 0.82) + (wet * 0.52);
    signal = LeakDC.ar(signal) * master * 5.0;
    Out.ar(out, Limiter.ar(signal, 0.82, 0.01));
});

checkpointDef = SynthDef(\refugeCheckpoint, { |out = 0|
    var master;
    var impactEnv;
    var impact;
    var whooshEnv;
    var whooshCenter;
    var whoosh;
    var shimmerEnv;
    var shimmerExciter;
    var shimmer;
    var haloEnv;
    var halo;
    var dry;
    var wet;
    var signal;

    master = EnvGen.ar(
        Env([0, 1, 1, 0], [0.018, 1.78, 0.28], [2, 0, -5]),
        doneAction: 2
    );

    impactEnv = EnvGen.ar(Env.perc(0.008, 0.72, curve: -6));
    impact = SinOsc.ar(XLine.kr(111, 98, 0.24), 0, 0.095) * impactEnv;
    impact = impact + (BPF.ar(PinkNoise.ar(0.28), 235, 0.78) * impactEnv * 0.34);

    whooshEnv = EnvGen.ar(Env([0, 1, 0.7, 0], [0.055, 0.58, 1.03], [-2, -1, -5]));
    whooshCenter = EnvGen.kr(Env([230, 1580, 650], [0.48, 1.12], [3, -4]));
    whoosh = BPF.ar(PinkNoise.ar(0.78), whooshCenter, 0.78) * whooshEnv;
    whoosh = LPF.ar(whoosh, 4700);

    shimmerEnv = EnvGen.ar(Env([0, 0, 1, 0.62, 0], [0.12, 0.19, 0.55, 0.72], [0, 3, -1, -5]));
    shimmerExciter = Dust2.ar(16) * shimmerEnv * 0.062;
    shimmer = Mix.ar(Ringz.ar(
        shimmerExciter,
        [783.99, 932.33, 1174.66, 1567.98, 2349.32, 3135.96],
        [0.13, 0.11, 0.14, 0.095, 0.075, 0.055]
    ));
    shimmer = LPF.ar(HPF.ar(shimmer, 700), 6250) * 0.16;

    haloEnv = EnvGen.ar(Env.perc(0.075, 1.42, curve: -4));
    halo = (SinOsc.ar(98.00, 0, 0.032) + SinOsc.ar(146.83, 0.4, 0.018)) * haloEnv;

    dry = Pan2.ar(impact, 0)
        + Pan2.ar(whoosh * 0.52, -0.1)
        + Pan2.ar(shimmer, 0.18)
        + Pan2.ar(halo, 0.04);
    wet = FreeVerb2.ar(dry[0], dry[1], mix: 0.4, room: 0.72, damp: 0.72);
    signal = (dry * 0.8) + (wet * 0.54);
    signal = LeakDC.ar(signal) * master * 4.5;
    Out.ar(out, Limiter.ar(signal, 0.82, 0.01));
});

render = { |definition, synthName, outputPath, duration, onComplete|
    var oscPath = outputPath ++ ".osc";
    var score = Score([
        [0.0, [\d_recv, definition.asBytes]],
        [0.01, [\s_new, synthName, 1000, 0, 0]],
        [duration, [\c_set, 0, 0]]
    ]);

    score.recordNRT(
        oscFilePath: oscPath,
        outputFilePath: outputPath,
        sampleRate: 48000,
        headerFormat: "WAV",
        sampleFormat: "int24",
        options: options,
        duration: duration,
        action: {
            File.delete(oscPath);
            onComplete.value;
        }
    );
};

("Rendering safe-area feedback to: " ++ safePath).postln;
render.(safeDef, \refugeSafeZone, safePath, 1.55, {
    ("Rendering checkpoint feedback to: " ++ checkpointPath).postln;
    render.(checkpointDef, \refugeCheckpoint, checkpointPath, 1.8, {
        "Refuge feedback renders complete.".postln;
        0.exit;
    });
});
)

/*
Generate_FluorescentBallastBuzz.scd

Renders three mono old-fluorescent-ballast buzz bodies for Don't Leave Me.
Wwise should control random selection, spacing, pitch, and 3D positioning.

Run from SuperCollider, or from a terminal:
  sclang Generate_FluorescentBallastBuzz.scd

Output: UnityProject_WwiseProject/Originals/SFX
Format: 48 kHz / 24-bit / mono WAV
*/

(
var scriptPath = thisProcess.nowExecutingPath;
var scriptDir = PathName(scriptPath).pathOnly;
var audioScriptsDir = PathName(scriptDir).parentPath;
var unityProjectDir = PathName(audioScriptsDir).parentPath;
var outputDir = unityProjectDir +/+ "UnityProject_WwiseProject" +/+ "Originals" +/+ "SFX";
var outputPaths = [
    outputDir +/+ "SFX_FluorescentBallast_Buzz_01_SC.wav",
    outputDir +/+ "SFX_FluorescentBallast_Buzz_02_SC.wav",
    outputDir +/+ "SFX_FluorescentBallast_Buzz_03_SC.wav"
];
var variants = [
    (duration: 1.36, seed: 1841, brightness: 0.92, roughness: 0.78),
    (duration: 1.82, seed: 2753, brightness: 1.08, roughness: 0.62),
    (duration: 0.94, seed: 3917, brightness: 0.84, roughness: 0.96)
];
var options;
var ballastDef;
var renderVariant;
var renderNext;

options = ServerOptions.new;
options.numOutputBusChannels = 1;
options.numInputBusChannels = 0;
options.sampleRate = 48000;
options.blockSize = 64;
options.memSize = 8192;

ballastDef = SynthDef(\fluorescentBallastBuzz, {
    |out = 0, duration = 1.3, seed = 1000, brightness = 1.0, roughness = 0.75|
    var master;
    var mainsDrift;
    var mains;
    var magneticRattle;
    var flicker;
    var buzzNoise;
    var buzzBands;
    var chatter;
    var signal;

    RandSeed.ir(1, seed);

    master = EnvGen.ar(
        Env([0, 1, 1, 0], [0.025, 0.89, 0.085], [3, 0, -5]),
        timeScale: duration,
        doneAction: 2
    );

    // A gently drifting 50 Hz foundation with transformer harmonics.
    mainsDrift = LFNoise1.kr(1.7).range(0.992, 1.008);
    mains = SinOsc.ar(50 * mainsDrift, 0, 0.052)
        + SinOsc.ar(100 * mainsDrift, 0.31, 0.030)
        + SinOsc.ar(150 * mainsDrift, 0.67, 0.014)
        + SinOsc.ar(250 * mainsDrift, 1.11, 0.006);

    // Uneven magnetic vibration: audible as an old ballast, not a clean synth.
    magneticRattle = LFPulse.ar(100 * mainsDrift, 0, 0.43, 0.055);
    magneticRattle = BPF.ar(magneticRattle, 410 + (roughness * 135), 0.42);
    magneticRattle = magneticRattle * LFNoise1.kr(7.3).range(0.62, 1.0);

    // Small voltage instability inside the body; Wwise owns the larger timing gaps.
    flicker = Lag.kr(
        LFDNoise0.kr(11 + (roughness * 8)).range(0.48, 1.0),
        0.018
    );
    buzzNoise = WhiteNoise.ar(0.12) + BrownNoise.ar(0.07);
    buzzBands = Mix.ar(BPF.ar(
        buzzNoise,
        [1180, 1860, 2840, 4180] * brightness,
        [0.12, 0.08, 0.065, 0.05],
        [0.38, 0.31, 0.20, 0.10]
    ));
    buzzBands = buzzBands * flicker;

    // Very quiet contact chatter adds age without becoming a spark or error beep.
    chatter = HPF.ar(Crackle.ar(1.66 + (roughness * 0.16), 0.018), 1450);
    chatter = LPF.ar(chatter, 5200 * brightness) * LFNoise1.kr(15).range(0.2, 0.75);

    signal = (mains * 0.72)
        + (magneticRattle * (0.55 + (roughness * 0.20)))
        + (buzzBands * 0.82)
        + chatter;
    signal = LPF.ar(LeakDC.ar(signal), 6100);
    signal = signal * master * 3.1;
    Out.ar(out, Limiter.ar(signal, 0.78, 0.01));
});

renderVariant = { |outputPath, variant, onComplete|
    var oscPath = outputPath ++ ".osc";
    var score = Score([
        [0.0, [\d_recv, ballastDef.asBytes]],
        [0.01, [\s_new, \fluorescentBallastBuzz, 1000, 0, 0,
            \duration, variant[\duration],
            \seed, variant[\seed],
            \brightness, variant[\brightness],
            \roughness, variant[\roughness]
        ]],
        [variant[\duration] + 0.04, [\c_set, 0, 0]]
    ]);

    score.recordNRT(
        oscFilePath: oscPath,
        outputFilePath: outputPath,
        sampleRate: 48000,
        headerFormat: "WAV",
        sampleFormat: "int24",
        options: options,
        duration: variant[\duration] + 0.04,
        action: {
            File.delete(oscPath);
            onComplete.value;
        }
    );
};

renderNext = { |index = 0|
    if(index >= variants.size, {
        "Fluorescent ballast renders complete.".postln;
        0.exit;
    }, {
        ("Rendering ballast variation " ++ (index + 1) ++ " to: " ++ outputPaths[index]).postln;
        renderVariant.(outputPaths[index], variants[index], {
            renderNext.(index + 1);
        });
    });
};

renderNext.(0);
)

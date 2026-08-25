/*
Generate_FluorescentBallastContinuousLoop.scd

Renders a quiet, seamless mono electrical bed for old fluorescent fixtures.
All oscillators and modulation rates complete whole cycles over ten seconds.
The script renders a short preroll, then trims exactly 480000 steady-state
samples so Wwise can loop the Sound SFX without a fade or silent boundary.

Run from SuperCollider, or from a terminal:
  sclang Generate_FluorescentBallastContinuousLoop.scd

Output: UnityProject_WwiseProject/Originals/SFX
Format: 48 kHz / 24-bit / mono WAV / exactly 10 seconds
*/

(
var scriptPath = thisProcess.nowExecutingPath;
var scriptDir = PathName(scriptPath).pathOnly;
var audioScriptsDir = PathName(scriptDir).parentPath;
var unityProjectDir = PathName(audioScriptsDir).parentPath;
var outputDir = unityProjectDir +/+ "UnityProject_WwiseProject" +/+ "Originals" +/+ "SFX";
var outputPath = outputDir +/+ "SFX_FluorescentLight_Continuous_Buzz_Loop_SC.wav";
var temporaryPath = outputPath ++ ".render.wav";
var oscPath = outputPath ++ ".osc";
var sampleRate = 48000;
var loopDuration = 10.0;
var preroll = 0.02;
var loopFrames = (sampleRate * loopDuration).asInteger;
var options;
var continuousDef;
var score;

options = ServerOptions.new;
options.numOutputBusChannels = 1;
options.numInputBusChannels = 0;
options.sampleRate = sampleRate;
options.blockSize = 64;
options.memSize = 8192;

continuousDef = SynthDef(\fluorescentBallastContinuous, { |out = 0|
    var drift;
    var voltage;
    var mains;
    var magneticBuzz;
    var transformerTexture;
    var contactWhine;
    var signal;

    // Every modulation frequency is a multiple of 0.1 Hz, so phase returns
    // exactly after the ten-second loop period.
    drift = SinOsc.kr(0.1, 0.15, 0.11)
        + SinOsc.kr(0.2, 2.1, 0.045)
        + SinOsc.kr(0.4, 4.2, 0.018);
    voltage = 0.82
        + SinOsc.kr(0.1, 1.7, 0.065)
        + SinOsc.kr(0.3, 4.4, 0.025)
        + SinOsc.kr(0.7, 0.8, 0.012);

    mains = SinOsc.ar(50 + drift, 0.18, 0.052)
        + SinOsc.ar((50 + drift) * 2, 0.56, 0.030)
        + SinOsc.ar((50 + drift) * 3, 1.12, 0.014)
        + SinOsc.ar((50 + drift) * 5, 2.03, 0.0055);
    mains = mains * voltage;

    // A restrained magnetic core vibration. VarSaw supplies the dry,
    // slightly mechanical edge of an old ballast without a bright spark.
    magneticBuzz = VarSaw.ar(
        100 + (drift * 2),
        iphase: 0.31,
        width: SinOsc.kr(0.2, 0.9, 0.035, 0.47),
        mul: 0.018
    );
    magneticBuzz = magneticBuzz * (
        0.72 + SinOsc.kr(0.5, 2.5, 0.11) + SinOsc.kr(0.9, 5.1, 0.045)
    );

    // Dense non-harmonic partials imitate transformer/circuit texture. Every
    // frequency has one decimal place, hence an integer cycle count in 10 s.
    transformerTexture = Mix.ar(SinOsc.ar(
        [
            1031.7, 1127.3, 1269.1, 1398.6, 1541.2, 1687.9,
            1849.4, 2033.8, 2247.1, 2479.6, 2713.3, 2981.7,
            3269.2, 3587.6, 3911.4, 4273.8, 4639.1, 4987.3
        ],
        [
            0.3, 2.2, 4.6, 1.1, 5.4, 3.2,
            0.8, 4.1, 2.7, 5.9, 1.8, 3.7,
            0.1, 4.9, 2.0, 5.1, 1.4, 3.0
        ],
        [
            0.0025, 0.0022, 0.0020, 0.0019, 0.0018, 0.0017,
            0.00155, 0.00145, 0.00135, 0.00125, 0.00115, 0.00105,
            0.00095, 0.00088, 0.00080, 0.00072, 0.00064, 0.00056
        ]
    ));
    transformerTexture = transformerTexture * (
        0.68 + SinOsc.kr(0.2, 3.4, 0.10) + SinOsc.kr(0.6, 0.7, 0.055)
    );

    contactWhine = SinOsc.ar(612.3, 0.9, 0.0032)
        + SinOsc.ar(823.7, 3.8, 0.0021)
        + SinOsc.ar(947.1, 5.2, 0.0014);
    contactWhine = contactWhine * (0.76 + SinOsc.kr(0.3, 1.2, 0.14));

    signal = mains + magneticBuzz + transformerTexture + contactWhine;
    // Memoryless saturation preserves the exact periodic boundary.
    signal = tanh(signal * 2.45) * 0.43;
    Out.ar(out, signal);
});

score = Score([
    [0.0, [\d_recv, continuousDef.asBytes]],
    [preroll, [\s_new, \fluorescentBallastContinuous, 1000, 0, 0]],
    [preroll + loopDuration + 0.03, [\c_set, 0, 0]]
]);

("Rendering continuous ballast loop to: " ++ outputPath).postln;
score.recordNRT(
    oscFilePath: oscPath,
    outputFilePath: temporaryPath,
    sampleRate: sampleRate,
    headerFormat: "WAV",
    sampleFormat: "float",
    options: options,
    duration: preroll + loopDuration + 0.03,
    action: {
        var input = SoundFile.new;
        var output;
        var data = FloatArray.newClear(loopFrames);
        var startFrame = (preroll * sampleRate).asInteger;

        input.openRead(temporaryPath);
        input.seek(startFrame);
        input.readData(data);
        input.close;

        output = SoundFile.openWrite(outputPath, "WAV", "int24", 1, sampleRate);
        output.writeData(data);
        output.close;

        File.delete(temporaryPath);
        File.delete(oscPath);
        "Continuous ballast loop render complete.".postln;
        0.exit;
    }
);
)

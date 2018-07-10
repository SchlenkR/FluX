namespace FLooping.Blocks

open System
open FLooping
open FLooping.Math


[<AutoOpen>]
module Filters =

    type BiQuadCoeffs = {
        a0: float;
        a1: float;
        a2: float;
        b1: float;
        b2: float;
        z1: float;
        z2: float;
    }

    type BiQuadParams = {
        q: float;
        frq: float;
        gain: float;
    }

    let biQuadCoeffsZero = { a0=0.0; a1=0.0; a2=0.0; b1=0.0; b2=0.0; z1=0.0; z2=0.0 }

    (*
        These implementations are based on http://www.earlevel.com/main/2011/01/02/biquad-formulas/
        and on https://raw.githubusercontent.com/filoe/cscore/master/CSCore/DSP
    *)

    let biQuadBase input (filterParams:BiQuadParams) (calcCoeffs:Env->BiQuadCoeffs) =
        let f p r =
            // seed: if we are run the first time, use default values for lastParams+lastCoeffs
            let lastParams,lastCoeffs =
                match p with
                | None ->   
                    (
                        filterParams,
                        calcCoeffs r
                    )
                | Some t -> t
            
            // calc the coeffs new if filter params have changed
            let coeffs =
                match lastParams = filterParams with
                | true -> lastCoeffs
                | false -> calcCoeffs r
            
            let o = input * coeffs.a0 + coeffs.z1
            let z1 = input * coeffs.a1 + coeffs.z2 - coeffs.b1 * o
            let z2 = input * coeffs.a2 - coeffs.b2 * o
            
            let newCoeffs = { coeffs with z1=z1; z2=z2 }

            { value=o; state=(filterParams,newCoeffs) }
        L f

    let lowPassDef = { frq=1000.0; q=1.0; gain=0.0 }
    let lowPass input (p:BiQuadParams) =
        let calcCoeffs (env:Env) =
            let k = Math.Tan(pi * p.frq / float env.sampleRate)
            let norm = 1.0 / (1.0 + k / p.q + k * k)
            let a0 = k * k * norm
            let a1 = 2.0 * a0
            let a2 = a0
            let b1 = 2.0 * (k * k - 1.0) * norm
            let b2 = (1.0 - k / p.q + k * k) * norm
            { biQuadCoeffsZero with a0=a0; a1=a1; a2=a2; b1=b1; b2=b2 }
        biQuadBase input p calcCoeffs

    let bandPassDef = { frq=1000.0; q=1.0; gain=0.0 }
    let bandPass input (p:BiQuadParams) =
        let calcCoeffs (env:Env) =
            let k = Math.Tan(pi * p.frq / float env.sampleRate)
            let norm = 1.0 / (1.0 + k / p.q + k * k)
            let a0 = k / p.q * norm
            let a1 = 0.0
            let a2 = -a0
            let b1 = 2.0 * (k * k - 1.0) * norm
            let b2 = (1.0 - k / p.q + k * k) * norm
            { biQuadCoeffsZero with a0=a0; a1=a1; a2=a2; b1=b1; b2=b2 }
        biQuadBase input p calcCoeffs

    let highShelfDef = { frq=1000.0; q=1.0; gain=0.0 }
    let highShelf input (p:BiQuadParams) =
        let calcCoeffs (env:Env) =
            let k = Math.Tan(Math.PI * p.frq / float env.sampleRate)
            let v = Math.Pow(10.0, Math.Abs(p.gain) / 20.0)
            match p.gain >= 0.0 with
            | true ->
                // boost
                let norm = 1.0 / (1.0 + sqrt2 * k + k * k)
                { biQuadCoeffsZero with 
                    a0 = (v + Math.Sqrt(2.0 * v) * k + k * k) * norm
                    a1 = 2.0 * (k * k - v) * norm
                    a2 = (v - Math.Sqrt(2.0 * v) * k + k * k) * norm
                    b1 = 2.0 * (k * k - 1.0) * norm
                    b2 = (1.0 - sqrt2 * k + k * k) * norm
                }
            | false ->
                // cut
                let norm = 1.0 / (v + Math.Sqrt(2.0 * v) * k + k * k)
                { biQuadCoeffsZero with 
                    a0 = (1.0 + sqrt2 * k + k * k) * norm
                    a1 = 2.0 * (k * k - 1.0) * norm
                    a2 = (1.0 - sqrt2 * k + k * k) * norm
                    b1 = 2.0 * (k * k - v) * norm
                    b2 = (v - Math.Sqrt(2.0 * v) * k + k * k) * norm
                }
        biQuadBase input p calcCoeffs

    let hishPassDef = { frq=1000.0; q=1.0; gain=0.0 }
    let highPass input (p:BiQuadParams) =
        let calcCoeffs (env:Env) =
            let k = Math.Tan(Math.PI * p.frq / float env.sampleRate)
            let norm = 1.0 / (1.0 + k / p.q + k * k)
            let a0 = norm
            let a1 = -2.0 * a0
            let a2 = a0
            let b1 = 2.0 * (k * k - 1.0) * norm
            let b2 = (1.0 - k / p.q + k * k) * norm
            { biQuadCoeffsZero with a0=a0; a1=a1; a2=a2; b1=b1; b2=b2 }
        biQuadBase input p calcCoeffs


        // protected override void CalculateBiQuadCoefficients()
        // {
        //     const double sqrt2 = 1.4142135623730951;
        //     double k = Math.Tan(Math.PI * Frequency / SampleRate);
        //     double v = Math.Pow(10, Math.Abs(GainDB) / 20.0);
        //     double norm;
        //     if (GainDB >= 0)
        //     {    // boost
        //         norm = 1 / (1 + sqrt2 * k + k * k);
        //         A0 = (1 + Math.Sqrt(2 * v) * k + v * k * k) * norm;
        //         A1 = 2 * (v * k * k - 1) * norm;
        //         A2 = (1 - Math.Sqrt(2 * v) * k + v * k * k) * norm;
        //         B1 = 2 * (k * k - 1) * norm;
        //         B2 = (1 - sqrt2 * k + k * k) * norm;
        //     }
        //     else
        //     {    // cut
        //         norm = 1 / (1 + Math.Sqrt(2 * v) * k + v * k * k);
        //         A0 = (1 + sqrt2 * k + k * k) * norm;
        //         A1 = 2 * (k * k - 1) * norm;
        //         A2 = (1 - sqrt2 * k + k * k) * norm;
        //         B1 = 2 * (v * k * k - 1) * norm;
        //         B2 = (1 - Math.Sqrt(2 * v) * k + v * k * k) * norm;
        //     }
        // }
    // let lowShelfDef = { frq=1000.0; q=1.0; gain=0.0 }
    // let lowShelf input (p:BiQuadParams) =
    //     let calcCoeffs (env:Env) =
    //         let k = Math.Tan(Math.PI * p.frq / float env.sampleRate)
    //         let v = Math.Pow(10.0, Math.Abs(p.gain) / 20.0)
    //         match p.gain >= 0.0 with
    //         | true ->
    //             // boost
    //             let norm = 1.0 / (1.0 + sqrt2 * k + k * k)
    //             { biQuadCoeffsZero with 
    //                 a0 = (v + Math.Sqrt(2.0 * v) * k + k * k) * norm
    //                 a1 = 2.0 * (k * k - v) * norm
    //                 a2 = (v - Math.Sqrt(2.0 * v) * k + k * k) * norm
    //                 b1 = 2.0 * (k * k - 1.0) * norm
    //                 b2 = (1.0 - sqrt2 * k + k * k) * norm
    //             }
    //         | false ->
    //             // cut
    //             let norm = 1.0 / (v + Math.Sqrt(2.0 * v) * k + k * k)
    //             { biQuadCoeffsZero with 
    //                 a0 = (1.0 + sqrt2 * k + k * k) * norm
    //                 a1 = 2.0 * (k * k - 1.0) * norm
    //                 a2 = (1.0 - sqrt2 * k + k * k) * norm
    //                 b1 = 2.0 * (k * k - v) * norm
    //                 b2 = (v - Math.Sqrt(2.0 * v) * k + k * k) * norm
    //             }
    //     biQuadBase input p calcCoeffs



// NOtch:
            // double k = Math.Tan(Math.PI * Frequency / SampleRate);
            // double norm = 1 / (1 + k / Q + k * k);
            // A0 = (1 + k * k) * norm;
            // A1 = 2 * (k * k - 1) * norm;
            // A2 = A0;
            // B1 = A1;
            // B2 = (1 - k / Q + k * k) * norm;



// Peak:
            // double norm;
            // double v = Math.Pow(10, Math.Abs(GainDB) / 20.0);
            // double k = Math.Tan(Math.PI * Frequency / SampleRate);
            // double q = Q;

            // if (GainDB >= 0) //boost
            // {
            //     norm = 1 / (1 + 1 / q * k + k * k);
            //     A0 = (1 + v / q * k + k * k) * norm;
            //     A1 = 2 * (k * k - 1) * norm;
            //     A2 = (1 - v / q * k + k * k) * norm;
            //     B1 = A1;
            //     B2 = (1 - 1 / q * k + k * k) * norm;
            // }
            // else //cut
            // {
            //     norm = 1 / (1 + v / q * k + k * k);
            //     A0 = (1 + 1 / q * k + k * k) * norm;
            //     A1 = 2 * (k * k - 1) * norm;
            //     A2 = (1 - 1 / q * k + k * k) * norm;
            //     B1 = A1;
            //     B2 = (1 - v / q * k + k * k) * norm;
            // }




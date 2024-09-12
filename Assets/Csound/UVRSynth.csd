<Cabbage>
form caption("Untitled") size(400, 700), guiMode("queue"), pluginId("def1"), colour("grey")
keyboard bounds(12, 96, 381, 95)
hslider bounds(28, 214, 150, 50) channel("vibrato") range(0, 1, 0, 1, 0.001) text("vibrato") popupText("vibrato"), textColour("white")
hslider bounds(28, 284, 150, 50) channel("pbUp") range(0, 1, 0, 1, 0.001) text("pbUp") popupText("pbUp"), textColour("white")
hslider bounds(204, 284, 150, 50) channel("pbDown") range(0, 1, 0, 1, 0.001) text("pbDown") popupText("pbDown"), textColour("white")
hslider bounds(32, 350, 145, 59) channel("filter") range(0, 1, 0.5, 1, 0.001) text("filter") popupText("filter"), textColour("white")
hslider bounds(206, 352, 145, 50) channel("rvbAmt") range(0, 1, 0.3, 1, 0.001) text("rvb amt") popupText("rvb amt"), textColour("white")
hslider bounds(34, 422, 150, 50) channel("delAmt") range(0, 1, 0.3, 1, 0.001) text("del amt") popupText("del amt"), textColour("white")
hslider bounds(206, 420, 150, 58) channel("delTime") range(0, 1, 0.2, 1, 0.001) text("delTime") popupText("delTime"), textColour("white")
hslider bounds(36, 494, 150, 50) channel("delFeedback") range(0, 1, 0.5, 1, 0.001) text("delFeedback") popupText("delFeedback"), textColour("white")
hslider bounds(34, 562, 150, 50) channel("distortion") range(0, 1, 0, 1, 0.001) text("distortion") popupText("distortion"), textColour("white")
hslider bounds(208, 552, 150, 50) channel("portTime") range(0, 1, 0.5, 1, 0.001) text("portTime") popupText("portTime"), textColour("white")


hslider bounds(36, 24, 150, 50) channel("freq") range(0, 15000, 55, 0.5, 0.001) text("freq") popupText("freq") , textColour("white")

checkbox bounds(244, 32, 100, 30) channel("noteon")
</Cabbage>
<CsoundSynthesizer>
<CsOptions>
-n -d -+rtmidi=NULL -M0 --midi-key-cps=4 --midi-velocity-amp=5
</CsOptions>
<CsInstruments>
; Initialize the global variables. 
ksmps = 32
nchnls = 2
0dbfs = 1


; initialize zak space
zakinit 3, 1

instr 22
    ; custom adsr based on chnget
    ktrigger init 0
    knoteon init 0
    knoteon = chnget:k("noteon")
    ktrigger = changed2:k(knoteon)
    kvol = 1 ;change for velocity input
    
    // write frequency to zak space
    kpitch = portk:k(chnget:k("freq"), chnget:k("portTime")/10)
    zkw kpitch, 1
    
    
    kamp init 0
    

    if ktrigger == 1 then
        if knoteon == 1 then
            event "i", 1, 0, -1 
        else
            event "i", -1, 0, 1
        endif
        ktrigger = 0
    endif
endin


gisine ftgen 0, 0, 2^10, 10, 1




//instrument will be triggered by keyboard widget
instr 1

    ; cps taken from zak space
    kfreq = zkr:k(1)
   
  ; From: oscilikt csound manual
  ; LFO
  kamp1 = chnget:k("vibrato") * (3+chnget:k("freq")/1500)
  kcps1 = chnget:k("vibrato") * 10
  itype = 0
  ksine lfo kamp1, kcps1, itype
  
  // envelope 
  kamp = madsr:k(0.1, 0.1, 0.8, 0.4)
  kpitchbend = 1+ chnget:k("pbUp")/8 - chnget:k("pbDown")/9
  
  ;random
  //krandpitch = randomi:k(1, 5, 1, 2, 3)
  
  // WGBOW -- SOUND GENERATOR
    /*kpres = 4							;pressure value
    krat = 0.127236						;position along string
    kvibf = 6.12723
		
    kvamp = kamp * 0.01
    aOutR  wgbow kamp*0.5, kfreq + ksine*kdistance, kpres, kpres, kvibf, kvamp, 1
    aOutL  wgbow kamp*0.5, kfreq + ksine*kdistance, kpres, kpres+0.5, kvibf, kvamp, 1*/
    
        // foscili -- Sound Generator
    aOutR poscil kamp/2, kfreq * kpitchbend + ksine, 1
    aOutL poscil kamp/2, kfreq * kpitchbend + ksine, 1
    
    
    //distortion
    kdist = port:k(chnget:k("distortion"), 0.01)
    //aOutR powershape aOutR, 1 + kdist* 2
    //aOutL powershape aOutL, 1+ kdist* 2
    aOutR distort aOutR, kdist, 3
    aOutL distort aOutL, kdist, 3
    
    //filter
    kfilterCutoff = 2^port:k(chnget:k("filter"), 0.01)-1
    aOutR zdf_ladder aOutR, 10000 * kfilterCutoff+kfreq,0.5
    aOutL zdf_ladder aOutL,10000 * kfilterCutoff+kfreq, 0.5
    
  //verb send
  krvbAmt = chnget:k("rvbAmt")
  zawm aOutL*krvbAmt, 1
  zawm aOutL*krvbAmt, 2
  
  //delay send
  kdelAmt = chnget:k("delAmt")
  aMono = (aOutL+aOutR) * 0.5
  zawm aOutR, 3
outs aOutL, aOutR
endin

//--------------------------------------------------------
//REVERB
instr 5 ; From: https://flossmanual.csound.com/sound-modification/reverberation
aInSigL       zar       1    ; read first zak audio channel
aInSigR       zar       2
denorm(aInSigL)
denorm(aInSigR)
kFblvl       init      0.88 ; feedback level - i.e. reverb time
kFco         init      8000 ; cutoff freq. of a filter within the reverb
; create reverberated version of input signal (note stereo input and output)
aRvbL,aRvbR  reverbsc  aInSigL, aInSigR, kFblvl, kFco
             outs      aRvbL, aRvbR ; send audio to outputs
             zacl      0, 2        ; clear zak audio channels
endin

//--------------------------------------------------------------------

instr 6 ;delay DISABLED
aInSig       zar       3    ; read second zak audio channel
adelAmt = a(port:k(chnget:k("delTime"), 0.01))

imd = 2 // max delay time in sec
iws = 1024 // interpolation window (4-1024) 
asig vdelayx aInSig, 1*adelAmt, imd, iws

// Set amount of delay
kdelAmp = port:k(chnget:k("delAmt"), 0.01)
asig = asig * kdelAmp

// Apply filter to delay output
kfreq = zkr:k(1)
asig = butterbp:a(asig, kfreq*2, kfreq*2)

outs asig, asig // output delay audio

zacl      3, 3         ; clear zak audio channels

kfbAmt = port:k(chnget:k("delFeedback"),0.01)
zawm asig*kfbAmt, 3 //feedback

endin
</CsInstruments>
<CsScore>
 //Table #1, a sine waveform.
f 1 0 4096 10 1 0 0 0.22 0 0.1 0.5 0 0.2
 //Table #2: a sawtooth wave
f 2 0 3 -2 1 0 -1
 //Table #3: sigmoid // from distort.csd in csound manual
f 3 0 257 9 .5 1 270	
;causes Csound to run for about 7000 years...
f0 z
i 22 0 z
i 5 0 z
i 6 0 z
</CsScore>
</CsoundSynthesizer>

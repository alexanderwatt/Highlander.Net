using System;
using nab.QR.Xml;
using System.Linq;

namespace nab.QR.PricingModelNab1
{
  internal class OptionMapping
  {
    public OptionType? OptionType { get; set; }
    public TriggerType? TriggerType { get; set; }
    public TrrigerLevelType? LevelType { get; set; }
    public TrrigerPeriodType? PeriodType{get;set;}
    public DigitalPayoffType? PayoffType{get;set;}
    public OptionCode OptionCode { get; set; }

    public bool Match(OptionType? optionType,
                      TriggerType? triggerType,
                      TrrigerLevelType? levelType,
                      TrrigerPeriodType? periodType,
                      DigitalPayoffType? payoffType)
    {
      return optionType == this.OptionType
          && triggerType == this.TriggerType
          && levelType == this.LevelType
          && periodType == this.PeriodType
          && payoffType == this.PayoffType;
    }
  }

  public static class OptionMappings
  {
    public static OptionCode Map(OptionType? optionType,
                                 TriggerType? triggerType,
                                 TrrigerLevelType? levelType,
                                 TrrigerPeriodType? periodType,
                                 DigitalPayoffType? payoffType)
    {
      OptionMapping mapping = _mappings.FirstOrDefault((x) => x.Match(optionType, triggerType, levelType, periodType, payoffType));
      return mapping == null ? OptionCode.ZZZZ : mapping.OptionCode;
    }

    public static bool IsPhysical(OptionCode optionCode)
    {
      return optionCode == OptionCode.FXSP
          || optionCode == OptionCode.FRWD
          || optionCode == OptionCode.ASST
          || optionCode == OptionCode.CASH;
    }

    public static bool IsBlackOption(OptionCode optionCode)
    {
      return optionCode == OptionCode.VEUC
          || optionCode == OptionCode.VEUP
          || optionCode == OptionCode.CPCC
          || optionCode == OptionCode.CPCP;
    }

    private static readonly OptionMapping[] _mappings = new OptionMapping[]{

    // Vanilla
    new OptionMapping {OptionType=OptionType.Call, OptionCode = OptionCode.VEUC},
    new OptionMapping {OptionType=OptionType.Put, OptionCode = OptionCode.VEUP},

    // Vanilla KI
    new OptionMapping {OptionType=OptionType.Call, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Lower, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.BDIC},
    new OptionMapping {OptionType=OptionType.Put, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Lower, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.BDIP},
    new OptionMapping {OptionType=OptionType.Call, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Upper, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.BUIC},
    new OptionMapping {OptionType=OptionType.Put, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Upper, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.BUIP},
    new OptionMapping {OptionType=OptionType.Call, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Both, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.B2IC},
    new OptionMapping {OptionType=OptionType.Put, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Both, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.B2IP},

    // Vanilla KO
    new OptionMapping {OptionType=OptionType.Call, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Lower, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.BDOC},
    new OptionMapping {OptionType=OptionType.Put, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Lower, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.BDOP},
    new OptionMapping {OptionType=OptionType.Call, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Upper, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.BUOC},
    new OptionMapping {OptionType=OptionType.Put, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Upper, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.BUOP},
    new OptionMapping {OptionType=OptionType.Call, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Both, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.B2OC},
    new OptionMapping {OptionType=OptionType.Put, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Both, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.B2OP},

    // Vanila KI Partial
    new OptionMapping {OptionType=OptionType.Call, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Lower, PeriodType=TrrigerPeriodType.Partial, OptionCode = OptionCode.SDIC},
    new OptionMapping {OptionType=OptionType.Put, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Lower, PeriodType=TrrigerPeriodType.Partial, OptionCode = OptionCode.SDIP},
    new OptionMapping {OptionType=OptionType.Call, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Upper, PeriodType=TrrigerPeriodType.Partial, OptionCode = OptionCode.SUIC},
    new OptionMapping {OptionType=OptionType.Put, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Upper, PeriodType=TrrigerPeriodType.Partial, OptionCode = OptionCode.SUIP},

    // Vanila KO Partial
    new OptionMapping {OptionType=OptionType.Call, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Lower, PeriodType=TrrigerPeriodType.Partial, OptionCode = OptionCode.SDOC},
    new OptionMapping {OptionType=OptionType.Put, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Lower, PeriodType=TrrigerPeriodType.Partial, OptionCode = OptionCode.SDOP},
    new OptionMapping {OptionType=OptionType.Call, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Upper, PeriodType=TrrigerPeriodType.Partial, OptionCode = OptionCode.SUOC},
    new OptionMapping {OptionType=OptionType.Put, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Upper, PeriodType=TrrigerPeriodType.Partial, OptionCode = OptionCode.SUOP},

    // Digital At-Expiry
    new OptionMapping {OptionType=OptionType.Call, PayoffType=DigitalPayoffType.Asset, OptionCode = OptionCode.EDAC},
    new OptionMapping {OptionType=OptionType.Put, PayoffType=DigitalPayoffType.Asset, OptionCode = OptionCode.EDAP},
    new OptionMapping {OptionType=OptionType.Call, PayoffType=DigitalPayoffType.Cash, OptionCode = OptionCode.EDNC},
    new OptionMapping {OptionType=OptionType.Put, PayoffType=DigitalPayoffType.Cash, OptionCode = OptionCode.EDNP},

    // Digital One-Touch
    new OptionMapping {PayoffType=DigitalPayoffType.Asset, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Lower, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.OTAP},
    new OptionMapping {PayoffType=DigitalPayoffType.Cash, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Lower, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.OTNP},
    new OptionMapping {PayoffType=DigitalPayoffType.Asset, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Upper, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.OTAC},
    new OptionMapping {PayoffType=DigitalPayoffType.Cash, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Upper, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.OTNC},
    new OptionMapping {PayoffType=DigitalPayoffType.Asset, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Both, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.RIAP},
    new OptionMapping {PayoffType=DigitalPayoffType.Cash, TriggerType=TriggerType.KI, LevelType=TrrigerLevelType.Both, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.RINP},

    // Digital No-Touch
    new OptionMapping {PayoffType=DigitalPayoffType.Asset, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Lower, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.NTAP},
    new OptionMapping {PayoffType=DigitalPayoffType.Cash, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Lower, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.NTNP},
    new OptionMapping {PayoffType=DigitalPayoffType.Asset, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Upper, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.NTAC},
    new OptionMapping {PayoffType=DigitalPayoffType.Cash, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Upper, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.NTNC},
    new OptionMapping {PayoffType=DigitalPayoffType.Asset, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Both, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.RBAP},
    new OptionMapping {PayoffType=DigitalPayoffType.Cash, TriggerType=TriggerType.KO, LevelType=TrrigerLevelType.Both, PeriodType=TrrigerPeriodType.Continuous, OptionCode = OptionCode.RBNP},

    };
  }
}

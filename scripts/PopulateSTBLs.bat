@echo off
pushd "%~dp0"

set MOD=..\strings\ServantRoleSims
set MAIN_LANG=ENG_US
set LIST=ENG_US CHS_CN CHT_CN CZE_CZ DAN_DK DUT_NL FIN_FI FRE_FR GER_DE GRE_GR HUN_HU ITA_IT JPN_JP KOR_KR NOR_NO POL_PL POR_PT POR_BR RUS_RU SPA_ES SPA_MX SWE_SE THA_TH

for %%i in (%LIST%) do (
    if not exist %MOD%_%%i.yaml (
        copy %MOD%_%MAIN_LANG%.yaml %MOD%_%%i.yaml
    )
)
popd

#!/bin/bash
# server=build.palaso.org
# project=Glyssen
# build=Glyssen master Continuous
# root_dir=..
# Auto-generated by https://github.com/chrisvire/BuildUpdate.
# Do not edit this file by hand!

cd "$(dirname "$0")"

# *** Functions ***
force=0
clean=0

while getopts fc opt; do
case $opt in
f) force=1 ;;
c) clean=1 ;;
esac
done

shift $((OPTIND - 1))

copy_auto() {
if [ "$clean" == "1" ]
then
echo cleaning $2
rm -f ""$2""
else
where_curl=$(type -P curl)
where_wget=$(type -P wget)
if [ "$where_curl" != "" ]
then
copy_curl "$1" "$2"
elif [ "$where_wget" != "" ]
then
copy_wget "$1" "$2"
else
echo "Missing curl or wget"
exit 1
fi
fi
}

copy_curl() {
echo "curl: $2 <= $1"
if [ -e "$2" ] && [ "$force" != "1" ]
then
curl -# -L -z "$2" -o "$2" "$1"
else
curl -# -L -o "$2" "$1"
fi
}

copy_wget() {
echo "wget: $2 <= $1"
f1=$(basename $1)
f2=$(basename $2)
cd $(dirname $2)
wget -q -L -N "$1"
# wget has no true equivalent of curl's -o option.
# Different versions of wget handle (or not) % escaping differently.
# A URL query is the only reason why $f1 and $f2 should differ.
if [ "$f1" != "$f2" ]; then mv $f2\?* $f2; fi
cd -
}


# *** Results ***
# build: Glyssen master Continuous (bt431)
# project: Glyssen
# URL: http://build.palaso.org/viewType.html?buildTypeId=bt431
# VCS: https://github.com/sillsdev/Glyssen.git [master]
# dependencies:
# [0] build: geckofx29-win32-continuous (bt399)
#     project: GeckoFx
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt399
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*"=>"lib/dotnet"}
#     VCS: https://bitbucket.org/geckofx/geckofx-29.0 [default]
# [1] build: XulRunner29-win32 (bt400)
#     project: GeckoFx
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt400
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"xulrunner-29.0.1.en-US.win32.zip!**"=>"lib"}
# [2] build: NetSparkle Continuous (NetSparkle_NetSparkleContinuous)
#     project: NetSparkle
#     URL: http://build.palaso.org/viewType.html?buildTypeId=NetSparkle_NetSparkleContinuous
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.dll"=>"lib/dotnet"}
#     VCS: https://github.com/sillsdev/NetSparkle [master]
# [3] build: palaso-Win-AnyCPU-master (Libpalaso_PalasoAnyCPUMaster)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=Libpalaso_PalasoAnyCPUMaster
#     clean: false
#     revision: glyssen-current.tcbuildtag
#     paths: {"icu.net.dll"=>"lib/dotnet", "x86/icu*.dll"=>"lib/dotnet", "L10NSharp.dll"=>"lib/dotnet", "L10NSharp.pdb"=>"lib/dotnet", "SIL.Core.dll"=>"lib/dotnet", "SIL.Core.pdb"=>"lib/dotnet", "SIL.Core.Desktop.dll"=>"lib/dotnet", "SIL.Core.Desktop.pdb"=>"lib/dotnet", "SIL.DblBundle.dll"=>"lib/dotnet", "SIL.DblBundle.pdb"=>"lib/dotnet", "SIL.DblBundle.Tests.dll"=>"lib/dotnet", "SIL.DblBundle.Tests.pdb"=>"lib/dotnet", "SIL.Scripture.dll"=>"lib/dotnet", "SIL.Scripture.pdb"=>"lib/dotnet", "SIL.TestUtilities.dll"=>"lib/dotnet", "SIL.Windows.Forms.dll"=>"lib/dotnet", "SIL.Windows.Forms.pdb"=>"lib/dotnet", "SIL.Windows.Forms.GeckoBrowserAdapter.dll"=>"lib/dotnet", "SIL.Windows.Forms.GeckoBrowserAdapter.pdb"=>"lib/dotnet", "SIL.Windows.Forms.Keyboarding.dll"=>"lib/dotnet", "SIL.Windows.Forms.Keyboarding.pdb"=>"lib/dotnet", "SIL.Windows.Forms.Scripture.dll"=>"lib/dotnet", "SIL.Windows.Forms.Scripture.pdb"=>"lib/dotnet", "SIL.Windows.Forms.WritingSystems.dll"=>"lib/dotnet", "SIL.Windows.Forms.WritingSystems.pdb"=>"lib/dotnet", "SIL.Windows.Forms.DblBundle.dll"=>"lib/dotnet", "SIL.Windows.Forms.DblBundle.pdb"=>"lib/dotnet", "SIL.WritingSystems.dll"=>"lib/dotnet", "SIL.WritingSystems.pdb"=>"lib/dotnet"}
#     VCS: https://github.com/sillsdev/libpalaso.git [master]

# make sure output directories exist
mkdir -p ../Downloads
mkdir -p ../lib
mkdir -p ../lib/dotnet

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt399/latest.lastSuccessful/Geckofx-Core.dll ../lib/dotnet/Geckofx-Core.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt399/latest.lastSuccessful/Geckofx-Core.dll.config ../lib/dotnet/Geckofx-Core.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt399/latest.lastSuccessful/Geckofx-Core.pdb ../lib/dotnet/Geckofx-Core.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt399/latest.lastSuccessful/Geckofx-Winforms.dll ../lib/dotnet/Geckofx-Winforms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt399/latest.lastSuccessful/Geckofx-Winforms.pdb ../lib/dotnet/Geckofx-Winforms.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt400/latest.lastSuccessful/xulrunner-29.0.1.en-US.win32.zip ../Downloads/xulrunner-29.0.1.en-US.win32.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/NetSparkle_NetSparkleContinuous/latest.lastSuccessful/NetSparkle.Net40.dll ../lib/dotnet/NetSparkle.Net40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterNostrongnameContinuous/latest.lastSuccessful/icu.net.dll ../lib/dotnet/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterNostrongnameContinuous/latest.lastSuccessful/x86/icudt56.dll ../lib/dotnet/icudt56.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterNostrongnameContinuous/latest.lastSuccessful/x86/icuin56.dll ../lib/dotnet/icuin56.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterNostrongnameContinuous/latest.lastSuccessful/x86/icuuc56.dll ../lib/dotnet/icuuc56.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterNostrongnameContinuous/latest.lastSuccessful/L10NSharp.dll ../lib/dotnet/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterNostrongnameContinuous/latest.lastSuccessful/L10NSharp.pdb ../lib/dotnet/L10NSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/L10NSharp.dll ../lib/dotnet/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/L10NSharp.pdb ../lib/dotnet/L10NSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Core.dll ../lib/dotnet/SIL.Core.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Core.pdb ../lib/dotnet/SIL.Core.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Core.Desktop.dll ../lib/dotnet/SIL.Core.Desktop.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Core.Desktop.pdb ../lib/dotnet/SIL.Core.Desktop.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.DblBundle.dll ../lib/dotnet/SIL.DblBundle.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.DblBundle.pdb ../lib/dotnet/SIL.DblBundle.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.DblBundle.Tests.dll ../lib/dotnet/SIL.DblBundle.Tests.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.DblBundle.Tests.pdb ../lib/dotnet/SIL.DblBundle.Tests.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Scripture.dll ../lib/dotnet/SIL.Scripture.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Scripture.pdb ../lib/dotnet/SIL.Scripture.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.TestUtilities.dll ../lib/dotnet/SIL.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Windows.Forms.dll ../lib/dotnet/SIL.Windows.Forms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Windows.Forms.pdb ../lib/dotnet/SIL.Windows.Forms.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Windows.Forms.GeckoBrowserAdapter.dll ../lib/dotnet/SIL.Windows.Forms.GeckoBrowserAdapter.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Windows.Forms.GeckoBrowserAdapter.pdb ../lib/dotnet/SIL.Windows.Forms.GeckoBrowserAdapter.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Windows.Forms.Keyboarding.dll ../lib/dotnet/SIL.Windows.Forms.Keyboarding.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Windows.Forms.Keyboarding.pdb ../lib/dotnet/SIL.Windows.Forms.Keyboarding.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Windows.Forms.Scripture.dll ../lib/dotnet/SIL.Windows.Forms.Scripture.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Windows.Forms.Scripture.pdb ../lib/dotnet/SIL.Windows.Forms.Scripture.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Windows.Forms.WritingSystems.dll ../lib/dotnet/SIL.Windows.Forms.WritingSystems.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Windows.Forms.WritingSystems.pdb ../lib/dotnet/SIL.Windows.Forms.WritingSystems.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Windows.Forms.DblBundle.dll ../lib/dotnet/SIL.Windows.Forms.DblBundle.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.Windows.Forms.DblBundle.pdb ../lib/dotnet/SIL.Windows.Forms.DblBundle.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.WritingSystems.dll ../lib/dotnet/SIL.WritingSystems.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoAnyCPUMaster/glyssen-current.tcbuildtag/SIL.WritingSystems.pdb ../lib/dotnet/SIL.WritingSystems.pdb
# extract downloaded zip files
unzip -uqo ../Downloads/xulrunner-29.0.1.en-US.win32.zip -d "../lib"
# End of script

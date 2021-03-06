#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

# Source this to add some fancy stuff to your scripts

COMMONSOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  COMMONDIR="$( cd -P "$( dirname "$COMMONSOURCE" )" && pwd )"
  COMMONSOURCE="$(readlink "$COMMONSOURCE")"
  [[ $COMMONSOURCE != /* ]] && COMMONSOURCE="$COMMONDIR/$COMMONSOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
COMMONDIR="$( cd -P "$( dirname "$COMMONSOURCE" )" && pwd )"

# Detect build servers
if [[ ! -z "$JENKINS_URL" || ! -z "$BUILD_BUILDID" ]]; then
    # Jenkins or VSO build, disable colors because they make things gross.
    NO_COLOR=1
fi

if [ "$NO_COLOR" != "1" ]; then
    # ANSI Colors
    RCol='\e[0m'    # Text Reset

    # Regular           Bold                Underline           High Intensity      BoldHigh Intens     Background          High Intensity Backgrounds
    Bla='\e[0;30m';     BBla='\e[1;30m';    UBla='\e[4;30m';    IBla='\e[0;90m';    BIBla='\e[1;90m';   On_Bla='\e[40m';    On_IBla='\e[0;100m';
    Red='\e[0;31m';     BRed='\e[1;31m';    URed='\e[4;31m';    IRed='\e[0;91m';    BIRed='\e[1;91m';   On_Red='\e[41m';    On_IRed='\e[0;101m';
    Gre='\e[0;32m';     BGre='\e[1;32m';    UGre='\e[4;32m';    IGre='\e[0;92m';    BIGre='\e[1;92m';   On_Gre='\e[42m';    On_IGre='\e[0;102m';
    Yel='\e[0;33m';     BYel='\e[1;33m';    UYel='\e[4;33m';    IYel='\e[0;93m';    BIYel='\e[1;93m';   On_Yel='\e[43m';    On_IYel='\e[0;103m';
    Blu='\e[0;34m';     BBlu='\e[1;34m';    UBlu='\e[4;34m';    IBlu='\e[0;94m';    BIBlu='\e[1;94m';   On_Blu='\e[44m';    On_IBlu='\e[0;104m';
    Pur='\e[0;35m';     BPur='\e[1;35m';    UPur='\e[4;35m';    IPur='\e[0;95m';    BIPur='\e[1;95m';   On_Pur='\e[45m';    On_IPur='\e[0;105m';
    Cya='\e[0;36m';     BCya='\e[1;36m';    UCya='\e[4;36m';    ICya='\e[0;96m';    BICya='\e[1;96m';   On_Cya='\e[46m';    On_ICya='\e[0;106m';
    Whi='\e[0;37m';     BWhi='\e[1;37m';    UWhi='\e[4;37m';    IWhi='\e[0;97m';    BIWhi='\e[1;97m';   On_Whi='\e[47m';    On_IWhi='\e[0;107m';
fi

cecho()
{
    local text=$1
    printf "%b\n" "$text"
}

header()
{
    local text=$1
    cecho "${BGre}*** $text ***${RCol}"
}

info()
{
    local text=$1
    cecho "${Gre}info :${RCol} $text"
}

warning()
{
    local text=$1
    cecho "${Yel}warn :${RCol} $text" 1>&2
}

error()
{
    local text=$1
    cecho "${Red}error:${RCol} $text" 1>&2
}

die()
{
    local text=$1
    error "$text"
    exit 1
}

export UNAME=$(uname)

if [ -z "$RID" ]; then
    if [ "$UNAME" == "Darwin" ]; then
        export OSNAME=osx
        export RID=osx.10.10-x64
        export DNX_FLAVOR="dnx-coreclr-darwin-x64"
    elif [ "$UNAME" == "Linux" ]; then
        # Detect Distro?
        export OSNAME=linux
        export RID=ubuntu.14.04-x64
        export DNX_FLAVOR="dnx-coreclr-linux-x64"
    else
        error "unknown OS: $UNAME" 1>&2
        exit 1
    fi
fi

export DNX_VERSION="1.0.0-rc1-update1"

export REPOROOT=$(cd $COMMONDIR/.. && pwd)
export OUTPUT_ROOT=$REPOROOT/artifacts/$RID
export DNX_DIR=$OUTPUT_ROOT/dnx
export STAGE1_DIR=$OUTPUT_ROOT/stage1
export STAGE2_DIR=$OUTPUT_ROOT/stage2
export HOST_DIR=$OUTPUT_ROOT/corehost

# TODO: Replace this with a dotnet generation
export TFM=dnxcore50

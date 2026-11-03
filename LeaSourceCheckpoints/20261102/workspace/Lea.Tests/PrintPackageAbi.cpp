#define _WIN32_WINNT 0x0602
#define NTDDI_VERSION 0x06020000
#include <windows.h>
#include <documenttarget.h>
#include <stddef.h>
#include <stdio.h>

static_assert(sizeof(PrintDocumentPackageStatus) == 24, "status ABI");
static_assert(offsetof(PrintDocumentPackageStatus, Completion) == 16, "completion ABI");
static_assert(offsetof(PrintDocumentPackageStatus, PackageStatus) == 20, "HRESULT ABI");

int main()
{
    wchar_t factory[40], factoryInterface[40], target[40], status[40];
    if (!StringFromGUID2(__uuidof(PrintDocumentPackageTargetFactory), factory, 40) ||
        !StringFromGUID2(__uuidof(IPrintDocumentPackageTargetFactory), factoryInterface, 40) ||
        !StringFromGUID2(__uuidof(IPrintDocumentPackageTarget), target, 40) ||
        !StringFromGUID2(__uuidof(IPrintDocumentPackageStatusEvent), status, 40)) return 1;
    wprintf(L"{\"factory\":\"%ls\",\"factoryInterface\":\"%ls\",\"target\":\"%ls\",\"status\":\"%ls\","
        L"\"statusSize\":%u,\"completionOffset\":%u,\"hresultOffset\":%u,\"messageSize\":%u,"
        L"\"completed\":%u,\"canceled\":%u,\"failed\":%u}\n",
        factory, factoryInterface, target, status,
        static_cast<unsigned>(sizeof(PrintDocumentPackageStatus)),
        static_cast<unsigned>(offsetof(PrintDocumentPackageStatus, Completion)),
        static_cast<unsigned>(offsetof(PrintDocumentPackageStatus, PackageStatus)),
        static_cast<unsigned>(sizeof(MSG)), static_cast<unsigned>(PrintDocumentPackageCompletion_Completed),
        static_cast<unsigned>(PrintDocumentPackageCompletion_Canceled), static_cast<unsigned>(PrintDocumentPackageCompletion_Failed));
    return 0;
}

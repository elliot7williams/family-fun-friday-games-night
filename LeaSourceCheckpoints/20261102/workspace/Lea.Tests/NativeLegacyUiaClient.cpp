#include <windows.h>
#include <uiautomation.h>
#include <tlhelp32.h>
#include <stdio.h>
#include <stdlib.h>
#include <wchar.h>
#include <string>
#include <wrl/client.h>

static void Trace(const char* value) { printf("NATIVE_LEGACY %s\n", value); fflush(stdout); }

static bool NoClr()
{
    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, GetCurrentProcessId());
    if (snapshot == INVALID_HANDLE_VALUE) return false;
    MODULEENTRY32W entry = {}; entry.dwSize = sizeof(entry);
    bool okay = true, found = Module32FirstW(snapshot, &entry) != FALSE;
    if (!found) okay = false;
    while (found) {
        if (!_wcsicmp(entry.szModule, L"clr.dll") || !_wcsicmp(entry.szModule, L"coreclr.dll") ||
            !_wcsicmp(entry.szModule, L"mscoree.dll") || !_wcsnicmp(entry.szModule, L"Lea.", 4)) okay = false;
        found = Module32NextW(snapshot, &entry) != FALSE;
    }
    if (GetLastError() != ERROR_NO_MORE_FILES) okay = false;
    CloseHandle(snapshot);
    return okay;
}

struct Resources
{
    HUIANODE node = nullptr;
    HUIAPATTERNOBJECT pattern = nullptr;
    HUIATEXTRANGE document = nullptr, visible = nullptr;
    SAFEARRAY* array = nullptr;
    VARIANT element;
    BSTR text = nullptr;
    Resources() { VariantInit(&element); }
    ~Resources() {
        SysFreeString(text);
        if (visible) UiaTextRangeRelease(visible);
        VariantClear(&element);
        if (array) SafeArrayDestroy(array);
        if (document) UiaTextRangeRelease(document);
        if (pattern) UiaPatternRelease(pattern);
        if (node) UiaNodeRelease(node);
    }
};

#ifdef NATIVE_EDIT_SERVER
static bool StopRequested(const wchar_t* path, const char* nonce)
{
    HANDLE file = CreateFileW(path, GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, nullptr);
    if (file == INVALID_HANDLE_VALUE) return false;
    LARGE_INTEGER size = {}; char buffer[32] = {}; DWORD count = 0;
    bool matched = GetFileSizeEx(file, &size) && size.QuadPart == 32 && ReadFile(file, buffer, 32, &count, nullptr) && count == 32 && !memcmp(buffer, nonce, 32);
    CloseHandle(file); return matched;
}

static int Server(wchar_t** argv)
{
    if (wcslen(argv[3]) != 32 || !NoClr()) return 10;
    char nonce[33] = {};
    for (int i = 0; i < 32; i++) {
        wchar_t c = argv[3][i];
        if (!((c >= L'0' && c <= L'9') || (c >= L'a' && c <= L'f'))) return 10;
        nonce[i] = static_cast<char>(c);
    }
    HWND parent = CreateWindowExW(0, L"STATIC", L"Native EDIT UIA baseline", WS_OVERLAPPEDWINDOW | WS_VISIBLE,
        CW_USEDEFAULT, CW_USEDEFAULT, 400, 180, nullptr, nullptr, GetModuleHandleW(nullptr), nullptr);
    if (!parent) return 11;
    HWND window = CreateWindowExW(0, L"EDIT", L"Alpha bravo minimal", WS_CHILD | WS_VISIBLE | ES_MULTILINE | ES_READONLY,
        0, 0, 360, 120, parent, nullptr, GetModuleHandleW(nullptr), nullptr);
    if (!window) { DestroyWindow(parent); return 11; }
    wchar_t text[64] = {};
    if (GetWindowTextW(window, text, 64) != 19 || wcscmp(text, L"Alpha bravo minimal") || !SetTimer(window, 1, 25, nullptr)) { DestroyWindow(parent); return 12; }
    char payload[256] = {};
    int length = sprintf_s(payload, "{\"handle\":%llu,\"pid\":%lu,\"nonce\":\"%s\"}",
        static_cast<unsigned long long>(reinterpret_cast<UINT_PTR>(window)), GetCurrentProcessId(), nonce);
    std::wstring temporary = std::wstring(argv[1]) + L".tmp";
    HANDLE file = CreateFileW(temporary.c_str(), GENERIC_WRITE, 0, nullptr, CREATE_NEW, FILE_ATTRIBUTE_NORMAL, nullptr);
    if (file == INVALID_HANDLE_VALUE) { DestroyWindow(parent); return 13; }
    DWORD written = 0;
    bool ready = length > 0 && WriteFile(file, payload, static_cast<DWORD>(length), &written, nullptr) && written == static_cast<DWORD>(length) && FlushFileBuffers(file);
    CloseHandle(file);
    if (!ready || !MoveFileW(temporary.c_str(), argv[1])) { DestroyWindow(parent); return 14; }
    Trace("standard EDIT server ready; no CLR or Lea modules");
    MSG message = {}; int status = 0;
    while (IsWindow(window) && (status = static_cast<int>(GetMessageW(&message, nullptr, 0, 0))) > 0) {
        if (message.message == WM_TIMER && StopRequested(argv[2], nonce)) { DestroyWindow(parent); break; }
        TranslateMessage(&message); DispatchMessageW(&message);
    }
    if (IsWindow(parent)) DestroyWindow(parent);
    bool clean = NoClr();
    if (clean) Trace("standard EDIT server final no CLR or Lea modules");
    Trace("standard EDIT server stopped");
    return status < 0 ? 15 : (clean ? 0 : 16);
}
#endif

#define CHECK_UIA(call) do { HRESULT result_ = (call); if (FAILED(result_)) return result_; } while (0)
#ifdef NATIVE_EDIT_SERVER
static HRESULT ModernControl(HWND window)
{
    using Microsoft::WRL::ComPtr;
    ComPtr<IUIAutomation> automation;
    CHECK_UIA(CoCreateInstance(__uuidof(CUIAutomation), nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&automation)));
    ComPtr<IUIAutomationElement> element;
    CHECK_UIA(automation->ElementFromHandle(window, &element));
    ComPtr<IUIAutomationTextPattern> pattern;
    CHECK_UIA(element->GetCurrentPatternAs(UIA_TextPatternId, IID_PPV_ARGS(&pattern)));
    if (!pattern) return E_NOINTERFACE;
    ComPtr<IUIAutomationTextRange> document;
    CHECK_UIA(pattern->get_DocumentRange(&document));
    BSTR text = nullptr;
    HRESULT read = document->GetText(-1, &text);
    bool matched = SUCCEEDED(read) && text && !wcscmp(text, L"Alpha bravo minimal");
    SysFreeString(text);
    if (!matched) return FAILED(read) ? read : E_FAIL;
    Trace("modern COM standard EDIT document exact 19 characters passed");
    ComPtr<IUIAutomationTextRangeArray> ranges;
    CHECK_UIA(pattern->GetVisibleRanges(&ranges));
    int count = 0; CHECK_UIA(ranges->get_Length(&count));
    if (count != 1) return E_FAIL;
    ComPtr<IUIAutomationTextRange> visible;
    CHECK_UIA(ranges->GetElement(0, &visible));
    text = nullptr; read = visible->GetText(-1, &text);
    matched = SUCCEEDED(read) && text && !wcscmp(text, L"Alpha bravo minimal");
    SysFreeString(text);
    if (!matched) return FAILED(read) ? read : E_FAIL;
    Trace("modern COM standard EDIT visible exact 19 characters passed");
    return S_OK;
}
#endif
static HRESULT Query(HWND window)
{
#ifdef NATIVE_EDIT_SERVER
    Trace("modern COM standard EDIT control");
    CHECK_UIA(ModernControl(window));
#endif
    Resources state;
    CHECK_UIA(UiaNodeFromHandle(window, &state.node));
    if (!state.node) return E_NOINTERFACE;
    Trace("legacy node nonnull");
    CHECK_UIA(UiaGetPatternProvider(state.node, UIA_TextPatternId, &state.pattern));
    if (!state.pattern) return E_NOINTERFACE;
    Trace("legacy text pattern nonnull");
    Trace("native document call");
    CHECK_UIA(TextPattern_get_DocumentRange(state.pattern, &state.document));
    if (!state.document) return E_NOINTERFACE;
    Trace("native document GetText call");
    CHECK_UIA(TextRange_GetText(state.document, -1, &state.text));
    if (!state.text || wcscmp(state.text, L"Alpha bravo minimal")) return E_FAIL;
    Trace("document exact 19 characters passed");
    SysFreeString(state.text); state.text = nullptr;
    CHECK_UIA(TextPattern_GetVisibleRanges(state.pattern, &state.array));
    if (!state.array || SafeArrayGetDim(state.array) != 1) return E_FAIL;
    VARTYPE type = VT_EMPTY;
    CHECK_UIA(SafeArrayGetVartype(state.array, &type));
    LONG first = 0, last = 0;
    CHECK_UIA(SafeArrayGetLBound(state.array, 1, &first));
    CHECK_UIA(SafeArrayGetUBound(state.array, 1, &last));
    if (type != VT_VARIANT || first != last) return E_FAIL;
    CHECK_UIA(SafeArrayGetElement(state.array, &first, &state.element));
    printf("NATIVE_LEGACY array type=%u element type=%u\n", type, state.element.vt); fflush(stdout);
    CHECK_UIA(UiaHTextRangeFromVariant(&state.element, &state.visible));
    if (!state.visible || !NoClr()) return E_FAIL;
    Trace("no CLR or Lea modules before visible read");
    Trace("visible native GetText call");
    CHECK_UIA(TextRange_GetText(state.visible, -1, &state.text));
    if (!state.text || wcscmp(state.text, L"Alpha bravo minimal")) return E_FAIL;
    Trace("visible exact 19 characters passed");
    return S_OK;
}

int wmain(int argc, wchar_t** argv)
{
#ifdef NATIVE_EDIT_SERVER
    if (argc == 5 && !wcscmp(argv[1], L"--server")) return Server(argv + 1);
#endif
    if (argc != 4 || wcscmp(argv[1], L"--client")) return 2;
    wchar_t* end = nullptr;
    unsigned __int64 raw = _wcstoui64(argv[2], &end, 10);
    if (!raw || !end || *end) return 3;
    unsigned long expected = wcstoul(argv[3], &end, 10);
    if (!expected || !end || *end) return 3;
    HWND window = reinterpret_cast<HWND>(static_cast<UINT_PTR>(raw));
    DWORD actual = 0;
    if (!GetWindowThreadProcessId(window, &actual) || actual != expected || actual == GetCurrentProcessId() || !NoClr()) return 4;
    Trace("separate provider/client processes; no CLR or Lea modules");
    HRESULT initialized = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    if (FAILED(initialized)) return 5;
    wchar_t path[MAX_PATH] = {};
    if (!GetModuleFileNameW(GetModuleHandleW(L"UIAutomationCore.dll"), path, MAX_PATH)) { CoUninitialize(); return 6; }
    printf("NATIVE_LEGACY core=%ls\n", path); fflush(stdout);
    HRESULT result = Query(window);
    bool clean = NoClr();
    CoUninitialize();
    printf("NATIVE_LEGACY HRESULT=%08lX\n", static_cast<unsigned long>(result)); fflush(stdout);
    if (FAILED(result) || !clean) return 7;
    Trace("PASS 4");
    return 0;
}

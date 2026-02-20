import { NextRequest, NextResponse } from "next/server";
import createMiddleware from "next-intl/middleware";
import { routing } from "./i18n/routing";

const intlMiddleware = createMiddleware(routing);

const PUBLIC_PATHS = [
  "/login",
  "/register",
  "/api",
];

function isPublicPath(pathname: string): boolean {
  // Strip locale prefix for matching
  const withoutLocale = pathname.replace(/^\/(nb|nn|en)/, "") || "/";
  return PUBLIC_PATHS.some((p) => withoutLocale.startsWith(p));
}

function getTokenFromRequest(request: NextRequest): string | null {
  // We store the refresh token in localStorage (client-side only)
  // For SSR middleware we check a cookie that the client sets after login
  return request.cookies.get("cf_has_session")?.value ?? null;
}

export default function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Run next-intl locale routing first
  const intlResponse = intlMiddleware(request);

  // Check if this is an auth-required route
  if (!isPublicPath(pathname)) {
    const hasSession = getTokenFromRequest(request);
    if (!hasSession) {
      const locale = pathname.split("/")[1] || "nb";
      const validLocales = ["nb", "nn", "en"];
      const detectedLocale = validLocales.includes(locale) ? locale : "nb";
      const loginUrl = new URL(`/${detectedLocale}/login`, request.url);
      loginUrl.searchParams.set("from", pathname);
      return NextResponse.redirect(loginUrl);
    }
  }

  return intlResponse;
}

export const config = {
  matcher: [
    // Match all paths except static files and Next.js internals
    "/((?!_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp)$).*)",
  ],
};

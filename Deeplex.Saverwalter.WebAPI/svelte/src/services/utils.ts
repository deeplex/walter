export function convertDate(date: Date | undefined): string | undefined {
  if (date) {
    // en-CA is yyyy-MM-dd, which is requested by C# DateOnly
    return date.toLocaleDateString('en-CA');
  } else {
    return undefined;
  }
}

export function convertTime(text: string | undefined): string | undefined {
  if (text) {
    return new Date(text).toLocaleString('de-DE');
  } else {
    return undefined;
  }
}

export function convertEuro(value: number | undefined): string | undefined {
  return `${(value || 0).toFixed(2)} €`;
}

export function convertPercent(value: number | undefined): string | undefined {
  return `${((value || 0) * 100).toFixed(2)}%`;
}

export function convertM2(value: number | undefined): string | undefined {
  return `${(value || 0).toFixed(2)} m²`;
}

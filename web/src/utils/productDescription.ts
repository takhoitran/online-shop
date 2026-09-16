export interface ProductSpecRow {
  label: string;
  value: string;
}

export interface ParsedProductDescription {
  /** Prose shown above the spec table. */
  intro: string | null;
  /** Rows parsed from description or supplied by caller. */
  specs: ProductSpecRow[];
}

const SPEC_SECTION_MARKERS = /^(\*\*)?(thông số|specs?|chi tiết sản phẩm)(\*\*)?\s*:?\s*$/i;

/**
 * Splits a free-text product description into intro + "Label: value" spec lines.
 * Also recognizes a dedicated specs section after a "Thông số:" / "Specs:" header line.
 */
export function parseProductDescription(description: string | null | undefined): ParsedProductDescription {
  if (!description?.trim()) {
    return { intro: null, specs: [] };
  }

  const lines = description.split(/\r?\n/).map((l) => l.trim()).filter(Boolean);
  const specHeaderIndex = lines.findIndex((line) => SPEC_SECTION_MARKERS.test(line));

  if (specHeaderIndex >= 0) {
    const introLines = lines.slice(0, specHeaderIndex);
    const specLines = lines.slice(specHeaderIndex + 1);
    return {
      intro: introLines.length ? introLines.join('\n\n') : null,
      specs: specLines.map(parseSpecLine).filter(Boolean) as ProductSpecRow[],
    };
  }

  const specRows: ProductSpecRow[] = [];
  const introLines: string[] = [];

  for (const line of lines) {
    const row = parseSpecLine(line);
    if (row) specRows.push(row);
    else introLines.push(line);
  }

  // Only treat colon-lines as specs when there are at least two — avoids false positives in prose.
  if (specRows.length >= 2) {
    return {
      intro: introLines.length ? introLines.join('\n\n') : null,
      specs: specRows,
    };
  }

  return { intro: description.trim(), specs: [] };
}

function parseSpecLine(line: string): ProductSpecRow | null {
  const match = line.match(/^([^:]{2,40}):\s*(.+)$/);
  if (!match) return null;
  return { label: match[1].trim(), value: match[2].trim() };
}

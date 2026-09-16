export interface SavedAddress {
  id: string;
  recipientName: string;
  phoneNumber: string;
  addressLine: string;
  city: string;
  isDefault?: boolean;
}

const STORAGE_KEY = 'onlineshop_saved_addresses';

function readRaw(): SavedAddress[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    const parsed = raw ? (JSON.parse(raw) as unknown) : [];
    if (!Array.isArray(parsed)) return [];
    return parsed.filter(
      (a): a is SavedAddress =>
        typeof a === 'object' &&
        a !== null &&
        typeof (a as SavedAddress).id === 'string' &&
        typeof (a as SavedAddress).recipientName === 'string' &&
        typeof (a as SavedAddress).phoneNumber === 'string' &&
        typeof (a as SavedAddress).addressLine === 'string' &&
        typeof (a as SavedAddress).city === 'string',
    );
  } catch {
    return [];
  }
}

function writeRaw(addresses: SavedAddress[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(addresses));
}

export function loadSavedAddresses(): SavedAddress[] {
  const items = readRaw();
  return [...items].sort((a, b) => Number(b.isDefault) - Number(a.isDefault));
}

export function saveAddressFromCheckout(input: Omit<SavedAddress, 'id' | 'isDefault'>, makeDefault = true): SavedAddress {
  const existing = readRaw();
  const duplicate = existing.find(
    (a) =>
      a.recipientName === input.recipientName &&
      a.phoneNumber === input.phoneNumber &&
      a.addressLine === input.addressLine &&
      a.city === input.city,
  );
  if (duplicate) {
    if (makeDefault) {
      const next = existing.map((a) => ({ ...a, isDefault: a.id === duplicate.id }));
      writeRaw(next);
    }
    return duplicate;
  }

  const created: SavedAddress = {
    id: crypto.randomUUID(),
    ...input,
    isDefault: makeDefault,
  };
  const next = makeDefault
    ? [created, ...existing.map((a) => ({ ...a, isDefault: false }))]
    : [...existing, created];
  writeRaw(next);
  return created;
}

export function removeSavedAddress(id: string) {
  writeRaw(readRaw().filter((a) => a.id !== id));
}

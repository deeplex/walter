// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

import { WalterAdresseEntry } from './WalterAdresse';
import { WalterApiHandler } from './WalterApiHandler';
import { WalterBankkontoEntry } from './WalterBankkonto';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterTransaktionEntry } from './WalterTransaktion';
import { WalterVertragEntry } from './WalterVertrag';
import { WalterWohnungEntry } from './WalterWohnung';

export interface WalterMitgliedschaftEntry {
    id: number;
    juristischePerson: WalterSelectionEntry;
    mitglied: WalterSelectionEntry;
    von: string;
    bis: string | undefined;
    anteil: number | undefined;
}

export class WalterKontaktEntry extends WalterApiHandler {
    public static ApiURL = `/api/kontakte`;

    constructor(
        public id: number,
        public email: string,
        public telefon: string,
        public fax: string,
        public mobil: string,
        public notiz: string,
        public rechtsform: WalterSelectionEntry,
        public bezeichnung: string,
        public name: string,
        public vorname: string,
        public createdAt: Date,
        public lastModified: Date,
        public anrede: WalterSelectionEntry,
        public selectedJuristischePersonen: WalterSelectionEntry[],
        public adresse: WalterAdresseEntry | undefined,
        public juristischePersonen: WalterKontaktEntry[],
        public wohnungen: WalterWohnungEntry[],
        public vertraege: WalterVertragEntry[],
        public selectedMitglieder: WalterSelectionEntry[],
        public mitglieder: WalterKontaktEntry[],
        public transaktionen: WalterTransaktionEntry[] = [],
        public bankkontos: WalterBankkontoEntry[] = [],
        public mitgliedschaftenAlsMitglied: WalterMitgliedschaftEntry[] = [],
        public mitgliedschaftenAlsJuristischePerson: WalterMitgliedschaftEntry[] = [],
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterKontaktEntry): WalterKontaktEntry {
        const selectedJuristischePersonen =
            json.selectedJuristischePersonen?.map(
                WalterSelectionEntry.fromJson
            );
        const adresse =
            json.adresse && WalterAdresseEntry.fromJson(json.adresse);
        const juristischePersonen = json.juristischePersonen?.map(
            WalterKontaktEntry.fromJson
        );
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);
        const vertraege = json.vertraege?.map(WalterVertragEntry.fromJson);
        const mitglieder = json.mitglieder?.map(WalterKontaktEntry.fromJson);
        const transaktionen = json.transaktionen?.map(
            WalterTransaktionEntry.fromJson
        );
        const bankkontos =
            json.bankkontos?.map(WalterBankkontoEntry.fromJson) ?? [];
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);
        const rechtsform =
            json.rechtsform && WalterSelectionEntry.fromJson(json.rechtsform);
        const selectedMitglieder = json.selectedMitglieder?.map(
            WalterSelectionEntry.fromJson
        );

        const mitgliedschaftenAlsMitglied: WalterMitgliedschaftEntry[] =
            json.mitgliedschaftenAlsMitglied?.map((m) => ({
                id: m.id,
                juristischePerson: WalterSelectionEntry.fromJson(m.juristischePerson),
                mitglied: WalterSelectionEntry.fromJson(m.mitglied),
                von: m.von,
                bis: m.bis ?? undefined,
                anteil: m.anteil ?? undefined
            })) ?? [];
        const mitgliedschaftenAlsJuristischePerson: WalterMitgliedschaftEntry[] =
            json.mitgliedschaftenAlsJuristischePerson?.map((m) => ({
                id: m.id,
                juristischePerson: WalterSelectionEntry.fromJson(m.juristischePerson),
                mitglied: WalterSelectionEntry.fromJson(m.mitglied),
                von: m.von,
                bis: m.bis ?? undefined,
                anteil: m.anteil ?? undefined
            })) ?? [];

        return new WalterKontaktEntry(
            json.id,
            json.email,
            json.telefon,
            json.fax,
            json.mobil,
            json.notiz,
            rechtsform,
            json.bezeichnung,
            json.name,
            json.vorname,
            json.createdAt,
            json.lastModified,
            json.anrede,
            selectedJuristischePersonen,
            adresse,
            juristischePersonen,
            wohnungen,
            vertraege,
            selectedMitglieder,
            mitglieder,
            transaktionen,
            bankkontos,
            mitgliedschaftenAlsMitglied,
            mitgliedschaftenAlsJuristischePerson,
            permissions
        );
    }
}

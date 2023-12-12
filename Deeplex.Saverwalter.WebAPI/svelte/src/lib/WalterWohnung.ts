import { WalterAdresseEntry } from './WalterAdresse';
import { WalterApiHandler } from './WalterApiHandler';
import { WalterBetriebskostenrechnungEntry } from './WalterBetriebskostenrechnung';
import { WalterErhaltungsaufwendungEntry } from './WalterErhaltungsaufwendung';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterUmlageEntry } from './WalterUmlage';
import { WalterVertragEntry } from './WalterVertrag';
import { WalterZaehlerEntry } from './WalterZaehler';

export class WalterWohnungEntry extends WalterApiHandler {
    public static ApiURL = `/api/wohnungen`;

    constructor(
        public adresse: WalterAdresseEntry,
        public id: number,
        public bezeichnung: string,
        public wohnflaeche: number,
        public nutzflaeche: number,
        public einheiten: number,
        public notiz: string,
        public createdAt: Date,
        public lastModified: Date,
        public besitzer: WalterSelectionEntry | undefined,
        public haus: WalterWohnungEntry[],
        public zaehler: WalterZaehlerEntry[],
        public vertraege: WalterVertragEntry[],
        public betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[],
        public erhaltungsaufwendungen: WalterErhaltungsaufwendungEntry[],
        public umlagen: WalterUmlageEntry[],
        public bewohner: string,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterWohnungEntry) {
        const adresse =
            json.adresse && WalterAdresseEntry.fromJson(json.adresse);
        const besitzer =
            json.besitzer && WalterSelectionEntry.fromJson(json.besitzer);
        const haus: WalterWohnungEntry[] = json.haus?.map(
            WalterWohnungEntry.fromJson
        );
        const zaehler = json.zaehler?.map(WalterZaehlerEntry.fromJson);
        const vertraege = json.vertraege?.map(WalterVertragEntry.fromJson);
        const betriebskostenrechnungen = json.betriebskostenrechnungen?.map(
            WalterBetriebskostenrechnungEntry.fromJson
        );
        const erhaltungsaufwendungen = json.erhaltungsaufwendungen?.map(
            WalterErhaltungsaufwendungEntry.fromJson
        );
        const umlagen = json.umlagen?.map(WalterUmlageEntry.fromJson);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterWohnungEntry(
            adresse,
            json.id,
            json.bezeichnung,
            json.wohnflaeche,
            json.nutzflaeche,
            json.einheiten,
            json.notiz,
            json.createdAt,
            json.lastModified,
            besitzer,
            haus,
            zaehler,
            vertraege,
            betriebskostenrechnungen,
            erhaltungsaufwendungen,
            umlagen,
            json.bewohner,
            permissions
        );
    }
}

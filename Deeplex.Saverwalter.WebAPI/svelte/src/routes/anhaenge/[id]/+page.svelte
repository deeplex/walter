<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import {
		WalterGrid,
		WalterHeader,
		WalterTextInput,
		WalterBetriebskostenrechnungen,
		WalterErhaltungsaufwendungen,
		WalterKontakte,
		WalterUmlagen,
		WalterVertraege,
		WalterWohnungen,
		WalterZaehlerList
	} from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type { WalterAnhangEntry } from '$WalterTypes';

	export let data: PageData;
	const a: Promise<WalterAnhangEntry> = walter_get(`/api/anhaenge/${data.id}`);
	const entry: Partial<WalterAnhangEntry> = {};
	a.then((e) => Object.assign(entry, e));
</script>

<WalterHeader title={a.then((x) => x.fileName)} />

<WalterGrid>
	<Row>
		<WalterTextInput
			bind:binding={entry.fileName}
			labelText="Dateiname"
			value={a.then((x) => x.fileName)}
		/>
		<WalterTextInput
			readonly
			labelText="Erstellungszeit"
			value={a.then((x) => x.creationTime)}
		/>
	</Row>
	<!-- <Row>
		<WalterTextInput labelText="Notiz" value={a.then((x) => x.notiz)} />
	</Row> -->
	<Accordion>
		<WalterBetriebskostenrechnungen
			title="Betriebskostenrechnungen"
			rows={a.then((x) => x.betriebskostenrechnungen)}
		/>
		<WalterErhaltungsaufwendungen
			title="Erhaltungsaufwendungen"
			rows={a.then((x) => x.erhaltungsaufwendungen)}
		/>
		<WalterKontakte
			title="Kontakte"
			rows={a.then((x) => x.natuerlichePersonen.concat(x.juristischePersonen))}
		/>
		<WalterUmlagen title="Umlagen" rows={a.then((x) => x.umlagen)} />
		<WalterVertraege title="Verträge" rows={a.then((x) => x.vertraege)} />
		<WalterWohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} />
		<WalterZaehlerList title="Zähler" rows={a.then((x) => x.zaehler)} />
	</Accordion>
</WalterGrid>

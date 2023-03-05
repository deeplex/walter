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

	export let data: PageData;
	const a = data.a;
</script>

<WalterHeader title={a.fileName} />

<WalterGrid>
	<Row>
		<WalterTextInput bind:value={a.fileName} labelText="Dateiname" />
		<WalterTextInput
			readonly
			labelText="Erstellungszeit"
			bind:value={a.creationTime}
		/>
	</Row>
	<!-- <Row>
		<WalterTextInput labelText="Notiz" value={a.then((x) => x.notiz)} />
	</Row> -->
	<Accordion>
		<WalterBetriebskostenrechnungen
			title="Betriebskostenrechnungen"
			rows={a.betriebskostenrechnungen}
		/>
		<WalterErhaltungsaufwendungen
			title="Erhaltungsaufwendungen"
			rows={a.erhaltungsaufwendungen}
		/>
		<WalterKontakte
			title="Kontakte"
			rows={a.natuerlichePersonen.concat(a.juristischePersonen)}
		/>
		<WalterUmlagen title="Umlagen" rows={a.umlagen} />
		<WalterVertraege title="Verträge" rows={a.vertraege} />
		<WalterWohnungen title="Wohnungen" rows={a.wohnungen} />
		<WalterZaehlerList title="Zähler" rows={a.zaehler} />
	</Accordion>
</WalterGrid>

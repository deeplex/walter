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
</script>

<WalterHeader title={data.a.fileName} />

<WalterGrid>
	<Row>
		<WalterTextInput bind:value={data.a.fileName} labelText="Dateiname" />
		<WalterTextInput
			readonly
			labelText="Erstellungszeit"
			bind:value={data.a.creationTime}
		/>
	</Row>
	<!-- <Row>
		<WalterTextInput labelText="Notiz" value={a.then((x) => x.notiz)} />
	</Row> -->
	<Accordion>
		<WalterBetriebskostenrechnungen
			betriebskostentypen={data.betriebskostentypen}
			umlagen={data.umlagen}
			title="Betriebskostenrechnungen"
			rows={data.a.betriebskostenrechnungen}
		/>
		<WalterErhaltungsaufwendungen
			title="Erhaltungsaufwendungen"
			rows={data.a.erhaltungsaufwendungen}
		/>
		<WalterKontakte
			title="Kontakte"
			rows={data.a.natuerlichePersonen.concat(data.a.juristischePersonen)}
		/>
		<WalterUmlagen title="Umlagen" rows={data.a.umlagen} />
		<WalterVertraege title="Verträge" rows={data.a.vertraege} />
		<WalterWohnungen title="Wohnungen" rows={data.a.wohnungen} />
		<WalterZaehlerList title="Zähler" rows={data.a.zaehler} />
	</Accordion>
</WalterGrid>

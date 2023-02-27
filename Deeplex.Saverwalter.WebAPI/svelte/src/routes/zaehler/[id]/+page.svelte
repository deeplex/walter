<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import {
		Adresse,
		Anhaenge,
		WalterGrid,
		WalterHeader,
		WalterTextInput,
		Zaehler,
		Zaehlerstaende
	} from '../../../components';
	import { walter_get } from '../../../services/utils';
	import type { ZaehlerEntry } from '../../../types/zaehler.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const a: Promise<ZaehlerEntry> = walter_get(`/api/zaehler/${data.id}`);
	const entry: Partial<ZaehlerEntry> = {};
	a.then((e) => Object.assign(entry, e));
</script>

<WalterHeader title={a.then((x) => x.kennnummer)}>
	<Anhaenge rows={a.then((x) => x.anhaenge)} />
</WalterHeader>
<WalterGrid>
	<Row>
		<WalterTextInput
			bind:binding={entry.kennnummer}
			labelText="Kennnummer"
			value={a.then((x) => x.kennnummer)}
		/>
		<!-- TODO -->
		<WalterTextInput labelText="Typ" value={a.then((x) => x.typ)} />
	</Row>
	<!-- TODO -->
	<Adresse adresse={a.then((x) => x.adresse)} />

	<Accordion>
		<Zaehlerstaende title="Zählerstände" rows={a.then((x) => x.staende)} />
		<Zaehler title="Einzelzähler" rows={a.then((x) => x.einzelzaehler)} />
	</Accordion>
</WalterGrid>

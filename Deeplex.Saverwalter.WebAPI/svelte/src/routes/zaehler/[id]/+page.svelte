<script lang="ts">
	import { Row } from 'carbon-components-svelte';
	import {
		Adresse,
		WalterGrid,
		WalterHeader,
		WalterTextInput
	} from '../../../components';
	import { walter_get } from '../../../services/utils';
	import type { ZaehlerEntry } from '../../../types/zaehler.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const a: Promise<ZaehlerEntry> = walter_get(`/api/zaehler/${data.id}`);
</script>

<WalterHeader title={a.then((x) => x.kennnummer)} />
<WalterGrid>
	<Row>
		<WalterTextInput
			labelText="Kennnummer"
			value={a.then((x) => x.kennnummer)}
		/>
		<WalterTextInput labelText="Typ" value={a.then((x) => x.typ)} />
	</Row>
	<Adresse adresse={a.then((x) => x.adresse)} />
</WalterGrid>

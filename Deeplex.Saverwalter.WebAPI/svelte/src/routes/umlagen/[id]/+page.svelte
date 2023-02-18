<script lang="ts">
	import {
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import WalterGrid from '../../../components/WalterGrid.svelte';
	import WalterHeader from '../../../components/WalterHeader.svelte';
	import WalterTextInput from '../../../components/WalterTextInput.svelte';
	import { walter_get } from '../../../services/utils';
	import type { UmlageEntry } from '../../../types/umlage.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const a: Promise<UmlageEntry> = walter_get(`/api/umlagen/${data.id}`);
</script>

<WalterHeader title={a.then((x) => x.typ + ' - ' + x.wohnungenBezeichnung)} />

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Bezeichnung" value={a.then((x) => x.typ)} />
		<WalterTextInput
			labelText="Wohnfläche"
			value={a.then((x) => x.wohnungenBezeichnung)}
		/>
	</Row>
	<Row>
		<WalterTextInput labelText="Notiz" value={a.then((x) => x.notiz)} />
	</Row>
</WalterGrid>

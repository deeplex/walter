<script lang="ts">
	import Person from '../../../../components/Person.svelte';
	import {
		Column,
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { JuristischePersonEntry } from '../../../../types/juristischeperson.type';
	import { walter_get } from '../../../../services/utils';
	import WalterHeader from '../../../../components/WalterHeader.svelte';
	import WalterGrid from '../../../../components/WalterGrid.svelte';
	import WalterTextInput from '../../../../components/WalterTextInput.svelte';

	export let data: PageData;
	const a: Promise<JuristischePersonEntry> = walter_get(
		`/api/kontakte/jur/${data.id}`
	);
</script>

<WalterHeader title={a.then((e) => e.name)} />

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Bezeichnung" value={a.then((x) => x.name)} />
	</Row>
	<Person person={a} />
</WalterGrid>

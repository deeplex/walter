<script lang="ts">
	import { Row } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { NatuerlichePersonEntry } from '../../../../types/natuerlicheperson.type';
	import { walter_get } from '../../../../services/utils';
	import {
		Person,
		WalterHeader,
		WalterGrid,
		WalterTextInput
	} from '../../../../components';

	export let data: PageData;
	const a: Promise<NatuerlichePersonEntry> = walter_get(
		`/api/kontakte/nat/${data.id}`
	);
</script>

<WalterHeader title={a.then((x) => x.name)} />

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Vorname" value={a.then((x) => x.vorname)} />
		<WalterTextInput labelText="Nachname" value={a.then((x) => x.nachname)} />
	</Row>
	<Person person={a} />
</WalterGrid>

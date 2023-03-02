<script lang="ts">
	import type { PageData } from './$types';

	import type { WalterErhaltungsaufwendungEntry } from '$WalterTypes';
	import { walter_get } from '$WalterServices/requests';
	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterErhaltungsaufwendung
	} from '$WalterComponents';

	export let data: PageData;
	const url = `/api/erhaltungsaufwendungen/${data.id}`;

	const a: Promise<WalterErhaltungsaufwendungEntry> = walter_get(url);
	const entry: Partial<WalterErhaltungsaufwendungEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.aussteller.text + ' - ' + x.bezeichnung);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterErhaltungsaufwendung {a} {entry} />
</WalterGrid>

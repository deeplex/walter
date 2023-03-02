<script lang="ts">
	import type { WalterErhaltungsaufwendungEntry } from '$WalterTypes';
	import {
		WalterGrid,
		WalterErhaltungsaufwendung,
		WalterHeaderNew
	} from '$WalterComponents';

	const url = `/api/erhaltungsaufwendungen`;
	const entry: Partial<WalterErhaltungsaufwendungEntry> = {};

	const title = 'Neue Erhaltungsaufwendung';
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterErhaltungsaufwendung {entry} />
</WalterGrid>
